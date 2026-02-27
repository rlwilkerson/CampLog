using CampLog.Tests.PlaywrightSpecs.PageObjects;
using Microsoft.Playwright;

namespace CampLog.Tests.PlaywrightSpecs;

/// <summary>
/// Authentication E2E tests. Cover redirect-to-Keycloak, login flow, header state.
/// Skip attribute kept so `dotnet test` passes without a running app.
/// Run with: dotnet test --filter "Category=PlaywrightUI"
/// </summary>
[Trait("Category", "PlaywrightUI")]
public class AuthTests : PlaywrightSetup
{
    // -----------------------------------------------------------------------
    // Unauthenticated state
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Unauthenticated_NavigatingToTrips_RedirectsToKeycloak()
    {
        await Page.GotoAsync($"{AppUrl}/Trips");
        await Page.WaitForURLAsync("**/realms/**", new PageWaitForURLOptions { Timeout = 10_000 });
        Assert.Contains("realms", Page.Url);
    }

    [Fact]
    public async Task Unauthenticated_NavigatingToHome_ShowsLoginLink()
    {
        await Page.GotoAsync(AppUrl);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        var loginLink = Page.Locator(".login-icon-link");
        Assert.True(await loginLink.IsVisibleAsync());
    }

    [Fact]
    public async Task Unauthenticated_HomePage_UserAvatarNotVisible()
    {
        await Page.GotoAsync(AppUrl);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        Assert.False(await Page.Locator(".user-avatar").IsVisibleAsync());
    }

    [Fact]
    public async Task Unauthenticated_ClickingLoginLink_RedirectsToKeycloak()
    {
        await Page.GotoAsync(AppUrl);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Page.Locator(".login-icon-link").ClickAsync();
        await Page.WaitForURLAsync("**/realms/**", new PageWaitForURLOptions { Timeout = 10_000 });
        Assert.Contains("realms", Page.Url);
    }

    [Fact]
    public async Task LoginUrl_RedirectsToKeycloakLoginPage()
    {
        var loginPage = new LoginPage(Page, AppUrl);
        await loginPage.GotoAsync();
        Assert.Contains("realms", Page.Url);
        // Keycloak login form fields should be present
        Assert.True(await loginPage.UsernameField.IsVisibleAsync());
        Assert.True(await loginPage.PasswordField.IsVisibleAsync());
    }

    // -----------------------------------------------------------------------
    // Authenticated state (requires test user in Keycloak)
    // -----------------------------------------------------------------------

    [Fact]
    public async Task AfterLogin_UserLandsOnAppPage()
    {
        await LoginAsync();
        Assert.StartsWith(AppUrl, Page.Url);
    }

    [Fact]
    public async Task AfterLogin_NavigatingToHome_UserAvatarIsVisible()
    {
        await LoginAsync();
        await Page.GotoAsync(AppUrl);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var avatar = Page.Locator(".user-avatar");
        await avatar.WaitForAsync(new LocatorWaitForOptions { Timeout = 5_000 });
        Assert.True(await avatar.IsVisibleAsync());
    }

    [Fact]
    public async Task AfterLogin_LoginIconLinkIsNotVisible()
    {
        await LoginAsync();
        await Page.GotoAsync(AppUrl);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        Assert.False(await Page.Locator(".login-icon-link").IsVisibleAsync());
    }

    [Fact]
    public async Task AfterLogin_TripsPage_IsAccessibleWithoutRedirect()
    {
        await LoginAsync();
        await Page.GotoAsync($"{AppUrl}/Trips");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        // Should stay on the Trips page (not redirected to Keycloak)
        Assert.Contains($"{AppUrl}/Trips", Page.Url);
    }
}
