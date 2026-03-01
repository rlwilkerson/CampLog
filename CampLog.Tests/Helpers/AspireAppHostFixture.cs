using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json;

namespace CampLog.Tests.Helpers;

/// <summary>
/// Ensures a reachable Aspire-hosted web app + Keycloak test user before Playwright tests execute.
/// </summary>
public sealed class AspireAppHostFixture : IAsyncLifetime
{
    private static readonly SemaphoreSlim InitLock = new(1, 1);
    private static readonly ConcurrentQueue<string> AppHostOutput = new();
    private static readonly HttpClient Http = CreateHttpClient();

    private static Process? _appHostProcess;
    private static bool _initialized;
    private static bool _ownsAppHostProcess;
    private static int _activeUsages;
    private bool _acquiredUsage;

    private static string _webBaseUrl = string.Empty;
    private static string _keycloakBaseUrl = string.Empty;

    public string WebBaseUrl => _webBaseUrl;
    public string TestUsername => Environment.GetEnvironmentVariable("CAMPLOG_TEST_USER") ?? "testuser@camplog.test";
    public string TestPassword => Environment.GetEnvironmentVariable("CAMPLOG_TEST_PASSWORD") ?? "testpass";

    public async Task InitializeAsync()
    {
        await EnsureInitializedAsync();
        Interlocked.Increment(ref _activeUsages);
        _acquiredUsage = true;
    }

    public async Task DisposeAsync()
    {
        if (!_acquiredUsage)
        {
            return;
        }

        _acquiredUsage = false;
        if (Interlocked.Decrement(ref _activeUsages) != 0 || !_ownsAppHostProcess)
        {
            return;
        }

        if (_appHostProcess is { HasExited: false })
        {
            _appHostProcess.Kill(entireProcessTree: true);
            await _appHostProcess.WaitForExitAsync();
        }

        _ownsAppHostProcess = false;
        _appHostProcess = null;
        _initialized = false;
    }

    private static async Task EnsureInitializedAsync()
    {
        if (_initialized)
        {
            return;
        }

        await InitLock.WaitAsync();
        try
        {
            if (_initialized)
            {
                return;
            }

            _keycloakBaseUrl = NormalizeBaseUrl(Environment.GetEnvironmentVariable("CAMPLOG_TEST_KEYCLOAK_URL") ?? "http://localhost:8080");
            var webCandidates = GetCandidateWebUrls().ToArray();

            if (!await TryResolveWebUrlAsync(webCandidates, TimeSpan.FromSeconds(10)))
            {
                if (ShouldAutostartAppHost())
                {
                    StartAppHost();
                }

                if (!await TryResolveWebUrlAsync(webCandidates, TimeSpan.FromSeconds(180)))
                {
                    throw new InvalidOperationException(
                        $"Playwright fixture could not reach a web host. Tried: {string.Join(", ", webCandidates)}. " +
                        $"Set CAMPLOG_TEST_BASE_URL or start 'aspire run' manually. AppHost output tail: {GetOutputTail()}");
                }
            }

            await EnsureKeycloakReadyAsync();
            await EnsureTestUserCanGetTokenAsync();
            _initialized = true;
        }
        finally
        {
            InitLock.Release();
        }
    }

    private static bool ShouldAutostartAppHost()
    {
        var value = Environment.GetEnvironmentVariable("CAMPLOG_TEST_AUTOSTART_APPHOST");
        return !string.Equals(value, "false", StringComparison.OrdinalIgnoreCase);
    }

    private static IEnumerable<string> GetCandidateWebUrls()
    {
        var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        AddCandidate(Environment.GetEnvironmentVariable("CAMPLOG_TEST_BASE_URL"), set);

        foreach (var launchSettings in new[]
        {
            ResolveRepoPath("CampLog.Web", "Properties", "launchSettings.json"),
            ResolveRepoPath("CampLog.Web2", "Properties", "launchSettings.json")
        })
        {
            foreach (var url in ReadHttpsUrlsFromLaunchSettings(launchSettings))
            {
                AddCandidate(url, set);
            }
        }

        if (set.Count == 0)
        {
            set.Add("https://localhost:7215");
        }

        return set;
    }

    private static void AddCandidate(string? candidate, ISet<string> targets)
    {
        if (string.IsNullOrWhiteSpace(candidate))
        {
            return;
        }

        var normalized = NormalizeBaseUrl(candidate);
        if (normalized.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            targets.Add(normalized);
        }
    }

    private static IEnumerable<string> ReadHttpsUrlsFromLaunchSettings(string path)
    {
        if (!File.Exists(path))
        {
            yield break;
        }

        using var stream = File.OpenRead(path);
        using var doc = JsonDocument.Parse(stream);

        if (!doc.RootElement.TryGetProperty("profiles", out var profiles))
        {
            yield break;
        }

        foreach (var profile in profiles.EnumerateObject())
        {
            if (!profile.Value.TryGetProperty("applicationUrl", out var urlElement))
            {
                continue;
            }

            var raw = urlElement.GetString();
            if (string.IsNullOrWhiteSpace(raw))
            {
                continue;
            }

            foreach (var split in raw.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                if (split.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    yield return split;
                }
            }
        }
    }

    private static async Task<bool> TryResolveWebUrlAsync(IEnumerable<string> candidates, TimeSpan timeout)
    {
        var until = DateTime.UtcNow + timeout;
        while (DateTime.UtcNow < until)
        {
            foreach (var candidate in candidates)
            {
                if (await IsWebReachableAsync(candidate))
                {
                    _webBaseUrl = NormalizeBaseUrl(candidate);
                    return true;
                }
            }

            await Task.Delay(1000);
        }

        return false;
    }

    private static async Task<bool> IsWebReachableAsync(string baseUrl)
    {
        try
        {
            using var response = await Http.GetAsync($"{NormalizeBaseUrl(baseUrl)}/");
            return (int)response.StatusCode < 500;
        }
        catch
        {
            return false;
        }
    }

    private static async Task EnsureKeycloakReadyAsync()
    {
        var until = DateTime.UtcNow + TimeSpan.FromSeconds(120);
        while (DateTime.UtcNow < until)
        {
            try
            {
                using var response = await Http.GetAsync($"{_keycloakBaseUrl}/realms/camplog/.well-known/openid-configuration");
                if (response.IsSuccessStatusCode)
                {
                    return;
                }
            }
            catch
            {
                // Keep polling until timeout.
            }

            await Task.Delay(1000);
        }

        throw new InvalidOperationException($"Keycloak did not become ready at {_keycloakBaseUrl}.");
    }

    private static async Task EnsureTestUserCanGetTokenAsync()
    {
        var username = Environment.GetEnvironmentVariable("CAMPLOG_TEST_USER") ?? "testuser@camplog.test";
        var password = Environment.GetEnvironmentVariable("CAMPLOG_TEST_PASSWORD") ?? "testpass";

        using var payload = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "password",
            ["client_id"] = "camplog-web",
            ["client_secret"] = "camplog-secret",
            ["username"] = username,
            ["password"] = password
        });

        using var response = await Http.PostAsync($"{_keycloakBaseUrl}/realms/camplog/protocol/openid-connect/token", payload);
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var details = await response.Content.ReadAsStringAsync();
        throw new InvalidOperationException(
            $"Keycloak test-user token request failed for '{username}' ({(int)response.StatusCode}). " +
            "Verify realm import test user credentials and client direct-access grants. " +
            $"Details: {details}");
    }

    private static void StartAppHost()
    {
        if (_appHostProcess is { HasExited: false })
        {
            return;
        }

        var appHostProject = ResolveRepoPath("CampLog.AppHost", "CampLog.AppHost.csproj");
        var startInfo = new ProcessStartInfo
        {
            FileName = "aspire",
            Arguments = $"run --project \"{appHostProject}\" --non-interactive",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        _appHostProcess = Process.Start(startInfo)
            ?? throw new InvalidOperationException("Failed to start Aspire apphost process.");
        _ownsAppHostProcess = true;

        _appHostProcess.OutputDataReceived += (_, e) => CaptureOutput(e.Data);
        _appHostProcess.ErrorDataReceived += (_, e) => CaptureOutput(e.Data);
        _appHostProcess.BeginOutputReadLine();
        _appHostProcess.BeginErrorReadLine();
    }

    private static void CaptureOutput(string? data)
    {
        if (string.IsNullOrWhiteSpace(data))
        {
            return;
        }

        AppHostOutput.Enqueue(data);
        while (AppHostOutput.Count > 80)
        {
            AppHostOutput.TryDequeue(out _);
        }
    }

    private static string GetOutputTail() => string.Join(" | ", AppHostOutput);

    private static string ResolveRepoPath(params string[] parts)
    {
        var path = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
        foreach (var part in parts)
        {
            path = Path.Combine(path, part);
        }

        return path;
    }

    private static string NormalizeBaseUrl(string value) => value.Trim().TrimEnd('/');

    private static HttpClient CreateHttpClient()
    {
        var handler = new HttpClientHandler
        {
            AllowAutoRedirect = false,
            ServerCertificateCustomValidationCallback = (_, _, _, _) => true
        };
        return new HttpClient(handler)
        {
            Timeout = TimeSpan.FromSeconds(5)
        };
    }
}
