using Microsoft.Playwright;

namespace CampLog.Tests.PlaywrightSpecs;

/// <summary>
/// Mobile viewport E2E tests at 390×844 (iPhone 14).
/// Verifies tab bar, FAB, and page content are correctly rendered at mobile size.
/// Run with: dotnet test --filter "Category=PlaywrightUI"
/// </summary>
[Trait("Category", "PlaywrightUI")]
public class MobileViewportTests : PlaywrightSetup
{
    private const int MobileWidth  = 390;
    private const int MobileHeight = 844;

    private async Task SetMobileViewportAsync() =>
        await Page.SetViewportSizeAsync(MobileWidth, MobileHeight);

    // -----------------------------------------------------------------------
    // Home page at mobile
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Mobile_HomePage_RendersHeroCard()
    {
        await SetMobileViewportAsync();
        await LoginAsync();
        await Page.GotoAsync(AppUrl);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        Assert.True(await Page.Locator(".hero-card").IsVisibleAsync());
    }

    [Fact]
    public async Task Mobile_HomePage_TabBarIsVisible()
    {
        await SetMobileViewportAsync();
        await LoginAsync();
        await Page.GotoAsync(AppUrl);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        Assert.True(await Page.Locator(".app-tabbar").IsVisibleAsync());
    }

    // -----------------------------------------------------------------------
    // Trips page at mobile
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Mobile_TripsPage_PageHeadingIsVisible()
    {
        await SetMobileViewportAsync();
        await LoginAndGoToTripsAsync();

        var heading = Page.Locator("h1");
        Assert.True(await heading.IsVisibleAsync());
        Assert.Equal("My Trips", (await heading.TextContentAsync())?.Trim());
    }

    [Fact]
    public async Task Mobile_TripsPage_TabBarIsVisible()
    {
        await SetMobileViewportAsync();
        await LoginAndGoToTripsAsync();
        Assert.True(await Page.Locator(".app-tabbar").IsVisibleAsync());
    }

    // -----------------------------------------------------------------------
    // Tab bar — fixed at bottom on mobile
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Mobile_TabBar_HasFixedPositionAtBottom()
    {
        await SetMobileViewportAsync();
        await LoginAndGoToTripsAsync();

        var tabBar = Page.Locator(".app-tabbar");

        var position = await tabBar.EvaluateAsync<string>("el => window.getComputedStyle(el).position");
        Assert.Equal("fixed", position);

        var bottom = await tabBar.EvaluateAsync<string>("el => window.getComputedStyle(el).bottom");
        Assert.Equal("0px", bottom);
    }

    [Fact]
    public async Task Mobile_TabBar_NotObscuredByViewport()
    {
        await SetMobileViewportAsync();
        await LoginAndGoToTripsAsync();

        var tabBar = Page.Locator(".app-tabbar");
        var box = await tabBar.BoundingBoxAsync();

        Assert.NotNull(box);
        // Tab bar bottom edge must be within the viewport height (+ small tolerance for safe-area-inset)
        Assert.True(box.Y + box.Height <= MobileHeight + 40, // 40px tolerance for safe-area
            $"Tab bar bottom ({box.Y + box.Height}) exceeds viewport height ({MobileHeight})");
    }

    [Fact]
    public async Task Mobile_AllThreeTabs_AreVisible()
    {
        await SetMobileViewportAsync();
        await LoginAndGoToTripsAsync();

        Assert.True(await Page.Locator(".app-tabbar a[href='/']").IsVisibleAsync());
        Assert.True(await Page.Locator(".app-tabbar a[href='/Trips']").IsVisibleAsync());
        Assert.True(await Page.Locator(".app-tabbar a[href='/Account/Login']").IsVisibleAsync());
    }

    // -----------------------------------------------------------------------
    // FAB at mobile
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Mobile_FABButton_IsVisible()
    {
        await SetMobileViewportAsync();
        await LoginAndGoToTripsAsync();
        Assert.True(await Page.Locator(".fab-add-trip").IsVisibleAsync());
    }

    [Fact]
    public async Task Mobile_FABButton_HasFixedPosition()
    {
        await SetMobileViewportAsync();
        await LoginAndGoToTripsAsync();

        var position = await Page.Locator(".fab-add-trip")
            .EvaluateAsync<string>("el => window.getComputedStyle(el).position");
        Assert.Equal("fixed", position);
    }

    [Fact]
    public async Task Mobile_FABButton_IsPositionedAboveTabBar()
    {
        await SetMobileViewportAsync();
        await LoginAndGoToTripsAsync();

        var fabBox    = await Page.Locator(".fab-add-trip").BoundingBoxAsync();
        var tabBarBox = await Page.Locator(".app-tabbar").BoundingBoxAsync();

        Assert.NotNull(fabBox);
        Assert.NotNull(tabBarBox);

        // FAB center Y should be above the tab bar top edge
        var fabCenterY = fabBox.Y + fabBox.Height / 2;
        Assert.True(fabCenterY < tabBarBox.Y,
            $"FAB center Y ({fabCenterY}) should be above tab bar top ({tabBarBox.Y})");
    }

    // -----------------------------------------------------------------------
    // Interaction at mobile
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Mobile_FABClick_OpensSheet()
    {
        await SetMobileViewportAsync();
        await LoginAndGoToTripsAsync();

        await Page.Locator(".fab-add-trip").ClickAsync();
        await Page.WaitForFunctionAsync(
            "document.body.classList.contains('sheet-open')",
            null, new PageWaitForFunctionOptions { Timeout = 5_000 });

        Assert.True(await Page.EvaluateAsync<bool>("() => document.body.classList.contains('sheet-open')"));
    }

    [Fact]
    public async Task Mobile_TopBar_IsVisible()
    {
        await SetMobileViewportAsync();
        await LoginAndGoToTripsAsync();
        Assert.True(await Page.Locator(".app-topbar").IsVisibleAsync());
    }
}
