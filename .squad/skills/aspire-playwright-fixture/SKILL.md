# SKILL: Playwright E2E Tests with Aspire AppHost

## Problem
Playwright end-to-end tests cannot connect to the application when run via `dotnet test` because:
1. xUnit test runner spawns isolated process without Aspire orchestration context
2. Tests hardcode application URL (e.g., `https://localhost:7215`) but app is not running
3. No mechanism to start AppHost before test execution or discover dynamic endpoints

**Error Signature:**
```
Microsoft.Playwright.PlaywrightException : net::ERR_CONNECTION_REFUSED at https://localhost:7215/Account/Login
```

## Solution Pattern: IClassFixture Integration

Use xUnit's `IClassFixture<T>` to orchestrate Aspire AppHost lifecycle around Playwright test classes.

### Implementation

#### 1. Create AppHost Fixture
```csharp
// File: CampLog.Tests/Fixtures/AspireAppHostFixture.cs
using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;

namespace CampLog.Tests.Fixtures;

/// <summary>
/// Manages Aspire AppHost lifecycle for integration/E2E tests.
/// Starts AppHost, waits for resources to be healthy, exposes endpoint URLs.
/// </summary>
public class AspireAppHostFixture : IAsyncLifetime
{
    private DistributedApplication? _app;
    private IResourceNotificationService? _notificationService;
    
    /// <summary>Web application HTTPS endpoint (e.g., https://localhost:7215)</summary>
    public string WebAppUrl { get; private set; } = string.Empty;
    
    /// <summary>Keycloak HTTP endpoint (e.g., http://localhost:8080)</summary>
    public string KeycloakUrl { get; private set; } = string.Empty;

    public async Task InitializeAsync()
    {
        // Build and start the AppHost
        var appHostBuilder = DistributedApplication.CreateBuilder(new DistributedApplicationOptions
        {
            Args = Array.Empty<string>(),
            DisableDashboard = true // No need for dashboard in tests
        });

        // Register resources (same as Program.cs in AppHost)
        // Option A: Reference actual AppHost project and call its builder method
        // Option B: Inline resource definitions for test isolation
        
        _app = appHostBuilder.Build();
        await _app.StartAsync();
        
        _notificationService = _app.Services.GetRequiredService<IResourceNotificationService>();
        
        // Wait for critical resources to be healthy
        await WaitForResourceHealthyAsync("web", TimeSpan.FromSeconds(60));
        await WaitForResourceHealthyAsync("keycloak", TimeSpan.FromSeconds(60));
        
        // Discover endpoint URLs from running resources
        var webResource = _app.Resources.OfType<ProjectResource>().First(r => r.Name == "web");
        var keycloakResource = _app.Resources.OfType<ContainerResource>().First(r => r.Name == "keycloak");
        
        WebAppUrl = webResource.GetEndpoint("https").Url;
        KeycloakUrl = keycloakResource.GetEndpoint("http").Url;
    }

    public async Task DisposeAsync()
    {
        if (_app != null)
        {
            await _app.StopAsync();
            await _app.DisposeAsync();
        }
    }
    
    private async Task WaitForResourceHealthyAsync(string resourceName, TimeSpan timeout)
    {
        if (_notificationService == null) throw new InvalidOperationException("AppHost not initialized");
        
        var cts = new CancellationTokenSource(timeout);
        await foreach (var notification in _notificationService.WatchAsync(resourceName, cts.Token))
        {
            if (notification.ResourceState == "Running" && notification.HealthState == "Healthy")
                return;
        }
        
        throw new TimeoutException($"Resource '{resourceName}' did not become healthy within {timeout}");
    }
}
```

#### 2. Update Playwright Base Class
```csharp
// File: CampLog.Tests/PlaywrightSpecs/PlaywrightSetup.cs
namespace CampLog.Tests.PlaywrightSpecs;

public abstract class PlaywrightSetup : IAsyncLifetime, IClassFixture<AspireAppHostFixture>
{
    protected readonly AspireAppHostFixture AppHostFixture;
    protected string AppUrl => AppHostFixture.WebAppUrl; // Dynamic URL from fixture
    
    protected IPlaywright PlaywrightInstance { get; private set; } = null!;
    protected IBrowser Browser { get; private set; } = null!;
    protected IBrowserContext Context { get; private set; } = null!;
    protected IPage Page { get; private set; } = null!;

    protected PlaywrightSetup(AspireAppHostFixture appHostFixture)
    {
        AppHostFixture = appHostFixture;
    }

    public async Task InitializeAsync()
    {
        PlaywrightInstance = await Playwright.CreateAsync();
        Browser = await PlaywrightInstance.Chromium.LaunchAsync(new() { Headless = true });
        Context = await Browser.NewContextAsync(new()
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
    
    // Login and helper methods remain unchanged...
}
```

#### 3. Test Class Usage
```csharp
// File: CampLog.Tests/PlaywrightSpecs/AuthTests.cs
namespace CampLog.Tests.PlaywrightSpecs;

[Trait("Category", "PlaywrightUI")]
public class AuthTests : PlaywrightSetup
{
    public AuthTests(AspireAppHostFixture appHostFixture) : base(appHostFixture) { }
    
    [Fact]
    public async Task Unauthenticated_NavigatingToHome_ShowsLoginLink()
    {
        await Page.GotoAsync(AppUrl); // Uses dynamic URL from fixture
        var loginLink = Page.Locator("a[href*='/Account/Login']");
        await Expect(loginLink).ToBeVisibleAsync();
    }
}
```

## Benefits
1. ✅ **AppHost starts once per test class** — xUnit shares fixture across all tests in class
2. ✅ **Dynamic endpoint discovery** — no hardcoded ports; works with Aspire port allocation
3. ✅ **Health check validation** — tests don't start until resources are truly ready
4. ✅ **Clean teardown** — AppHost stops after all tests in class complete
5. ✅ **Realistic environment** — tests run against full Aspire orchestration (Keycloak, PostgreSQL, etc.)

## Trade-offs
- **Slower test execution:** AppHost startup adds ~10-30s per test class (but only once, not per test)
- **Resource intensive:** Requires Docker containers + .NET processes running simultaneously
- **Debugging complexity:** Logs from AppHost/containers mixed with test output

## Alternative: Manual Prerequisites
For faster local dev iteration, document manual setup:

```bash
# Terminal 1: Start AppHost
cd CampLog.AppHost
aspire run

# Terminal 2: Run tests (after AppHost healthy)
cd CampLog.Tests
dotnet test --filter Category=PlaywrightUI
```

Keep hardcoded `AppUrl = "https://localhost:7215"` in `PlaywrightSetup.cs` for this workflow.

**Trade-off:** Developers must remember to start AppHost manually; CI pipelines need orchestration logic.

## When to Use
- ✅ CI/CD pipelines running full E2E test suite
- ✅ Pre-merge quality gates requiring UI validation
- ✅ Projects where Playwright tests need to verify cross-resource behavior (API + Web + Keycloak)

## When NOT to Use
- ❌ Rapid TDD cycles on UI-only changes (too slow; use manual AppHost + test watch)
- ❌ Unit tests (no external dependencies; use in-memory fakes)
- ❌ API integration tests (use WebApplicationFactory instead)

## Prerequisites
- **Keycloak test user seeding:** Fixture must seed test user (`testuser@camplog.test / testpass`) during `InitializeAsync` or use realm import
- **Testcontainers compatibility:** Docker must be running for PostgreSQL container resource
- **Port availability:** Ensure Aspire-allocated ports (e.g., 7215, 8080, 5432) are not in use

## Related Patterns
- **WebApplicationFactory:** For API-only integration tests without full AppHost
- **Testcontainers:** For isolated service tests (e.g., PostgreSQL repository tests)
- **Playwright standalone:** For frontend-only tests against deployed environments

## References
- [xUnit Shared Context (IClassFixture)](https://xunit.net/docs/shared-context)
- [Aspire Testing Guidance](https://learn.microsoft.com/dotnet/aspire/testing)
- [Playwright .NET](https://playwright.dev/dotnet)

## CampLog Implementation Update (2026-02-28)

- Implemented fixture file: `CampLog.Tests/Helpers/AspireAppHostFixture.cs`.
- Playwright base class now uses fixture to resolve URL dynamically instead of hardcoding `https://localhost:7215`.
- URL resolution order:
  1. `CAMPLOG_TEST_BASE_URL`
  2. HTTPS entries from `CampLog.Web/Properties/launchSettings.json`
  3. HTTPS entries from `CampLog.Web2/Properties/launchSettings.json`
- Fixture defaults:
  - Auto-start AppHost (`aspire run --project CampLog.AppHost/CampLog.AppHost.csproj --non-interactive`) unless `CAMPLOG_TEST_AUTOSTART_APPHOST=false`.
  - Validate Keycloak readiness at `CAMPLOG_TEST_KEYCLOAK_URL` (default `http://localhost:8080`).
  - Validate token minting for `CAMPLOG_TEST_USER` / `CAMPLOG_TEST_PASSWORD` (defaults: `testuser@camplog.test` / `testpass`).

### Required Keycloak Realm Alignment
- `camplog-web` client must have `"directAccessGrantsEnabled": true` for token validation.
- Realm import test user credentials must match fixture defaults or explicit env overrides.

---
**Created by:** Wedge (QA/Tester)  
**Date:** 2026-02-28  
**Project:** CampLog
