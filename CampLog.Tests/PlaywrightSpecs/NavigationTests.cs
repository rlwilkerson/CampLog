using CampLog.Tests.PlaywrightSpecs.PageObjects;
using Microsoft.Playwright;

namespace CampLog.Tests.PlaywrightSpecs;

/// <summary>
/// Navigation E2E tests. Cover tab bar visibility, tab routing, active-state highlighting.
/// Run with: dotnet test --filter "Category=PlaywrightUI"
/// </summary>
[Trait("Category", "PlaywrightUI")]
public class NavigationTests : PlaywrightSetup
{
    // -----------------------------------------------------------------------
    // Tab bar visibility
    // -----------------------------------------------------------------------

    [Fact]
    public async Task TabBar_IsVisibleOnHomePage()
    {
        await LoginAsync();
        await Page.GotoAsync(AppUrl);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        var basePage = new HomePage(Page, AppUrl);
        Assert.True(await basePage.IsTabBarVisibleAsync());
    }

    [Fact]
    public async Task TabBar_IsVisibleOnTripsPage()
    {
        await LoginAndGoToTripsAsync();
        var basePage = new TripListPage(Page, AppUrl);
        Assert.True(await basePage.IsTabBarVisibleAsync());
    }

    [Fact]
    public async Task TabBar_ContainsThreeTabs()
    {
        await LoginAndGoToTripsAsync();
        var tabs = Page.Locator(".app-tabbar .tab-link");
        Assert.Equal(3, await tabs.CountAsync());
    }

    [Fact]
    public async Task TabBar_HasHomeTripsAndProfileTabs()
    {
        await LoginAndGoToTripsAsync();
        Assert.True(await Page.Locator(".app-tabbar a[href='/']").IsVisibleAsync());
        Assert.True(await Page.Locator(".app-tabbar a[href='/Trips']").IsVisibleAsync());
        Assert.True(await Page.Locator(".app-tabbar a[href='/Account/Login']").IsVisibleAsync());
    }

    // -----------------------------------------------------------------------
    // Tab routing
    // -----------------------------------------------------------------------

    [Fact]
    public async Task HomeTab_Click_NavigatesToHomePage()
    {
        await LoginAndGoToTripsAsync();
        await Page.Locator(".app-tabbar a[href='/']").ClickAsync();
        await Page.WaitForURLAsync($"{AppUrl}/", new PageWaitForURLOptions { Timeout = 10_000 });
        Assert.True(Page.Url.TrimEnd('/').Equals(AppUrl.TrimEnd('/')) ||
                    Page.Url.EndsWith("/"));
    }

    [Fact]
    public async Task TripsTab_Click_NavigatesToTripsPage()
    {
        await LoginAsync();
        await Page.GotoAsync(AppUrl);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await Page.Locator(".app-tabbar a[href='/Trips']").ClickAsync();
        await Page.WaitForURLAsync($"{AppUrl}/Trips", new PageWaitForURLOptions { Timeout = 10_000 });
        Assert.Contains("/Trips", Page.Url);
    }

    [Fact]
    public async Task ProfileTab_Click_NavigatesToAccountPage()
    {
        await LoginAndGoToTripsAsync();
        await Page.Locator(".app-tabbar a[href='/Account/Login']").ClickAsync();
        await Page.WaitForURLAsync("**/Account/**", new PageWaitForURLOptions { Timeout = 10_000 });
        Assert.Contains("Account", Page.Url);
    }

    // -----------------------------------------------------------------------
    // Active-tab state
    // -----------------------------------------------------------------------

    [Fact]
    public async Task HomeTab_IsActive_WhenOnHomePage()
    {
        await LoginAsync();
        await Page.GotoAsync(AppUrl);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var homeTab = Page.Locator(".app-tabbar a[href='/']");
        var isActive = await homeTab.EvaluateAsync<bool>("el => el.classList.contains('is-active')");
        Assert.True(isActive);
    }

    [Fact]
    public async Task TripsTab_IsActive_WhenOnTripsPage()
    {
        await LoginAndGoToTripsAsync();

        var tripsTab = Page.Locator(".app-tabbar a[href='/Trips']");
        var isActive = await tripsTab.EvaluateAsync<bool>("el => el.classList.contains('is-active')");
        Assert.True(isActive);
    }

    [Fact]
    public async Task HomeTab_IsNotActive_WhenOnTripsPage()
    {
        await LoginAndGoToTripsAsync();

        var homeTab = Page.Locator(".app-tabbar a[href='/']");
        var isActive = await homeTab.EvaluateAsync<bool>("el => el.classList.contains('is-active')");
        Assert.False(isActive);
    }

    [Fact]
    public async Task TripsTab_IsNotActive_WhenOnHomePage()
    {
        await LoginAsync();
        await Page.GotoAsync(AppUrl);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var tripsTab = Page.Locator(".app-tabbar a[href='/Trips']");
        var isActive = await tripsTab.EvaluateAsync<bool>("el => el.classList.contains('is-active')");
        Assert.False(isActive);
    }

    // -----------------------------------------------------------------------
    // Brand link
    // -----------------------------------------------------------------------

    [Fact]
    public async Task BrandLink_Click_NavigatesToHomePage()
    {
        await LoginAndGoToTripsAsync();
        await Page.Locator(".brand-link").ClickAsync();
        await Page.WaitForURLAsync($"{AppUrl}/", new PageWaitForURLOptions { Timeout = 10_000 });
        Assert.True(Page.Url.TrimEnd('/').Equals(AppUrl.TrimEnd('/')));
    }

    [Fact]
    public async Task BrandLink_IsVisibleOnAllPages()
    {
        await LoginAndGoToTripsAsync();
        Assert.True(await Page.Locator(".brand-link").IsVisibleAsync());

        await Page.GotoAsync(AppUrl);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        Assert.True(await Page.Locator(".brand-link").IsVisibleAsync());
    }
}
