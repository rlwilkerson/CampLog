using Microsoft.Playwright;

namespace CampLog.Tests.PlaywrightSpecs;

/// <summary>
/// Test-first acceptance gates for implementing Web2 Variation A
/// (Heritage Header + Content Bands) without changing core user flows.
/// </summary>
[Trait("Category", "PlaywrightUI")]
[Trait("Category", "VariationA")]
public class Web2VariationAAcceptanceSpecs : PlaywrightSetup
{
    [Fact]
    public async Task HeaderNavigation_Unauthenticated_ShowsHomeTripsPrivacyAndLoginInMasthead()
    {
        await Page.GotoAsync(AppUrl);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var headerNav = Page.Locator(".app-topbar nav[aria-label='Primary']");
        Assert.True(await headerNav.IsVisibleAsync(), "Variation A requires primary navigation in the top masthead.");
        Assert.Equal(3, await headerNav.Locator("a").CountAsync());
        Assert.True(await headerNav.Locator("a[href='/']:has-text('Home')").IsVisibleAsync());
        Assert.True(await headerNav.Locator("a[href='/Trips']:has-text('Trips')").IsVisibleAsync());
        Assert.True(await headerNav.Locator("a[href='/Privacy']:has-text('Privacy')").IsVisibleAsync());

        Assert.True(await Page.Locator(".app-topbar a[href^='/Account/Login']").IsVisibleAsync());
        Assert.False(await Page.Locator(".app-topbar a[href='/Account/Profile']").IsVisibleAsync());
    }

    [Fact]
    public async Task HeaderNavigation_Authenticated_ShowsProfileInMasthead_AndHidesLogin()
    {
        await LoginAsync();
        await Page.GotoAsync(AppUrl);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        Assert.True(await Page.Locator(".app-topbar a[href='/Account/Profile']").IsVisibleAsync());
        Assert.False(await Page.Locator(".app-topbar a[href^='/Account/Login']").IsVisibleAsync());
    }

    [Fact]
    public async Task HeaderNavigation_LoginLink_KeepsLocalReturnUrlForCurrentPage()
    {
        await Page.GotoAsync($"{AppUrl}/Privacy");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var href = await Page.Locator(".app-topbar a[href^='/Account/Login']").GetAttributeAsync("href");
        Assert.NotNull(href);
        Assert.Contains("returnUrl=", href);
        Assert.Contains("%2FPrivacy", href);
    }

    [Fact]
    public async Task SlideOutPanel_CreateAndEditActions_OpenPanelWithoutFullPageNavigation()
    {
        await LoginAndGoToTripsAsync();
        var tripsUrl = Page.Url;

        await Page.Locator(".fab-add-trip").ClickAsync();
        await Page.WaitForSelectorAsync("#trip-form-container input[name='Input.Name']");
        Assert.Equal(tripsUrl, Page.Url);
        Assert.True(await Page.EvaluateAsync<bool>("() => document.body.classList.contains('sheet-open')"));
        Assert.Equal(0, await Page.Locator("dialog[open], .modal, .pico-modal").CountAsync());

        if (await Page.Locator(".trip-item").CountAsync() == 0)
        {
            await Page.FillAsync("#trip-form-container input[name='Input.Name']", $"Variation A {Guid.NewGuid().ToString("N")[..8]}");
            await Page.Locator("#trip-form-container [type='submit']").ClickAsync();
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        }

        await Page.Locator(".muted-action:has-text('Edit')").First.ClickAsync();
        await Page.WaitForSelectorAsync("#trip-form-container input[name='Input.Name']");
        Assert.Contains("/Trips", Page.Url);
        Assert.True(await Page.EvaluateAsync<bool>("() => document.body.classList.contains('sheet-open')"));
    }

    [Fact]
    public async Task SlideOutPanel_CloseButtonOverlayAndEscape_AllCloseAndClearPanelContent()
    {
        await LoginAndGoToTripsAsync();

        await Page.Locator(".fab-add-trip").ClickAsync();
        await Page.WaitForSelectorAsync("#trip-form-container input[name='Input.Name']");
        await Page.Locator(".sheet-close").ClickAsync();
        await Page.WaitForFunctionAsync("!document.body.classList.contains('sheet-open')");

        await Page.Locator(".fab-add-trip").ClickAsync();
        await Page.WaitForSelectorAsync("#trip-form-container input[name='Input.Name']");
        await Page.Locator("#sheet-overlay").ClickAsync(new LocatorClickOptions { Force = true });
        await Page.WaitForFunctionAsync("!document.body.classList.contains('sheet-open')");

        await Page.Locator(".fab-add-trip").ClickAsync();
        await Page.WaitForSelectorAsync("#trip-form-container input[name='Input.Name']");
        await Page.Keyboard.PressAsync("Escape");
        await Page.WaitForFunctionAsync("!document.body.classList.contains('sheet-open')");

        Assert.Equal(string.Empty, (await Page.Locator("#trip-form-container").InnerTextAsync()).Trim());
    }

    [Fact]
    public async Task CoreParity_TripsLocationsAndLoginFlow_RemainIntactAfterVariationA()
    {
        await Page.GotoAsync(AppUrl);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        Assert.True(await Page.Locator("a[role='button'][href*='Account/Login'], a[role='button'][href='/Trips']").First.IsVisibleAsync());

        await LoginAndGoToTripsAsync();
        Assert.Equal("My Trips", (await Page.Locator("h1").TextContentAsync())?.Trim());

        if (await Page.Locator(".trip-item").CountAsync() == 0)
        {
            await CreateTripAsync($"Parity Trip {Guid.NewGuid().ToString("N")[..8]}", "Variation A parity");
        }

        await Page.Locator(".trip-main-link").First.ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        Assert.Contains("/Trips/Locations/", Page.Url);
        Assert.Equal("Locations", (await Page.Locator("h1").TextContentAsync())?.Trim());
        Assert.True(await Page.Locator("a[role='button']:has-text('Add Location')").IsVisibleAsync());
    }

    [Fact]
    public async Task ResponsiveAndVisual_DesktopUsesTopMastheadNav_NotBottomOrRailTabbar()
    {
        await LoginAsync();
        await Page.SetViewportSizeAsync(1280, 900);
        await Page.GotoAsync($"{AppUrl}/Trips");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        Assert.True(await Page.Locator(".app-topbar nav[aria-label='Primary']").IsVisibleAsync());
        Assert.False(await Page.Locator(".app-tabbar").IsVisibleAsync());
    }

    [Fact]
    public async Task ResponsiveAndVisual_MobileMastheadHasNoHorizontalOverflow_AndTouchTargetsAreAccessible()
    {
        await Page.SetViewportSizeAsync(390, 844);
        await Page.GotoAsync(AppUrl);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var dimensions = await Page.EvaluateAsync<int[]>(
            "() => [document.documentElement.scrollWidth, document.documentElement.clientWidth]");
        Assert.True(dimensions[0] <= dimensions[1] + 1,
            $"Expected no horizontal overflow at 390px, but scrollWidth={dimensions[0]} and clientWidth={dimensions[1]}.");

        var accountAction = Page.Locator(".app-topbar a[href^='/Account/Login'], .app-topbar a[href='/Account/Profile']").First;
        var box = await accountAction.BoundingBoxAsync();
        Assert.NotNull(box);
        Assert.True(box.Width >= 44, $"Expected account target width >= 44px, got {box.Width}px.");
        Assert.True(box.Height >= 44, $"Expected account target height >= 44px, got {box.Height}px.");
    }

    [Fact]
    public async Task ResponsiveAndVisual_DustySummerPaletteTokens_AreUnchanged()
    {
        await Page.GotoAsync(AppUrl);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var palette = await Page.EvaluateAsync<Dictionary<string, string>>(
            """
            () => {
              const styles = window.getComputedStyle(document.documentElement);
              return {
                salmon: styles.getPropertyValue('--color-salmon').trim().toLowerCase(),
                rose: styles.getPropertyValue('--color-rose').trim().toLowerCase(),
                terracotta: styles.getPropertyValue('--color-terracotta').trim().toLowerCase(),
                amber: styles.getPropertyValue('--color-amber').trim().toLowerCase(),
                inverse: styles.getPropertyValue('--pico-primary-inverse').trim().toLowerCase()
              };
            }
            """);

        Assert.Equal("#e3aa99", palette["salmon"]);
        Assert.Equal("#cd9f8f", palette["rose"]);
        Assert.Equal("#dc7147", palette["terracotta"]);
        Assert.Equal("#d8a748", palette["amber"]);
        Assert.Equal("#ffffff", palette["inverse"]);
    }
}
