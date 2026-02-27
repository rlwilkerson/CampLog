using Microsoft.Playwright;

namespace CampLog.Tests.PlaywrightSpecs;

/// <summary>
/// Base class for Playwright UI tests. Manages browser lifecycle and provides login helpers.
/// Tests require: app running at https://localhost:7215 + Keycloak.
/// Credentials: env vars CAMPLOG_TEST_USER / CAMPLOG_TEST_PASSWORD (default: testuser / testpass).
/// </summary>
public abstract class PlaywrightSetup : IAsyncLifetime
{
    protected const string AppUrl = "https://localhost:7215";

    protected static readonly string TestUsername =
        Environment.GetEnvironmentVariable("CAMPLOG_TEST_USER") ?? "testuser@camplog.test";

    protected static readonly string TestPassword =
        Environment.GetEnvironmentVariable("CAMPLOG_TEST_PASSWORD") ?? "testpass";

    protected IPlaywright PlaywrightInstance { get; private set; } = null!;
    protected IBrowser Browser { get; private set; } = null!;
    protected IBrowserContext Context { get; private set; } = null!;
    protected IPage Page { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        PlaywrightInstance = await Playwright.CreateAsync();
        Browser = await PlaywrightInstance.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true
        });
        Context = await Browser.NewContextAsync(new BrowserNewContextOptions
        {
            IgnoreHTTPSErrors = true,
            ViewportSize = new ViewportSize { Width = 1280, Height = 800 }
        });
        Page = await Context.NewPageAsync();
    }

    public async Task DisposeAsync()
    {
        await Context.CloseAsync();
        await Browser.CloseAsync();
        PlaywrightInstance.Dispose();
    }

    /// <summary>Navigates to /Account/Login, waits for Keycloak redirect, then logs in with test credentials.</summary>
    protected async Task LoginAsync()
    {
        await Page.GotoAsync($"{AppUrl}/Account/Login");
        await Page.WaitForURLAsync("**/realms/**", new PageWaitForURLOptions { Timeout = 15_000 });
        await Page.FillAsync("#username", TestUsername);
        await Page.FillAsync("#password", TestPassword);
        await Page.Locator("[type=submit]").First.ClickAsync();
        await Page.WaitForURLAsync($"{AppUrl}/**", new PageWaitForURLOptions { Timeout = 15_000 });
    }

    /// <summary>Logs in then navigates to /Trips and waits for network idle.</summary>
    protected async Task LoginAndGoToTripsAsync()
    {
        await LoginAsync();
        await Page.GotoAsync($"{AppUrl}/Trips");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    /// <summary>Opens the FAB sheet and waits for the form to be loaded by HTMX.</summary>
    protected async Task OpenCreateSheetAsync()
    {
        await Page.Locator(".fab-add-trip").ClickAsync();
        await Page.WaitForFunctionAsync(
            "document.body.classList.contains('sheet-open')",
            null,
            new PageWaitForFunctionOptions { Timeout = 5_000 });
        await Page.WaitForSelectorAsync("#trip-form-container input[name='Input.Name']");
    }

    /// <summary>Creates a trip via the FAB sheet and waits for the list to refresh.</summary>
    protected async Task<string> CreateTripAsync(string name = "", string description = "", string startDate = "2025-07-04")
    {
        var tripName = string.IsNullOrEmpty(name) ? $"E2E Trip {Guid.NewGuid():N[..8]}" : name;
        await OpenCreateSheetAsync();
        await Page.FillAsync("#trip-form-container input[name='Input.Name']", tripName);
        if (!string.IsNullOrEmpty(description))
            await Page.FillAsync("#trip-form-container textarea[name='Input.Description']", description);
        await Page.FillAsync("#trip-form-container input[name='Input.StartDate']", startDate);
        await Page.Locator("#trip-form-container [type=submit]").ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        return tripName;
    }
}
