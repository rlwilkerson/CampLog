using Microsoft.Playwright;

namespace CampLog.Tests.PlaywrightSpecs;

/// <summary>
/// Acceptance criteria for the frontend modernization effort.
/// These tests define QA gates for layout/spacing/typography/navigation/responsive behavior,
/// while enforcing that the Dusty Summer color palette remains unchanged.
/// </summary>
[Trait("Category", "PlaywrightUI")]
public class FrontendRedesignAcceptanceTests : PlaywrightSetup
{
    [Fact]
    public async Task ThemePalette_DustySummerColors_ArePreserved()
    {
        await LoginAsync();
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

    [Fact]
    public async Task HomeHero_ModernSpacingAndTypography_MeetsReadabilityCriteria()
    {
        await LoginAsync();
        await Page.GotoAsync(AppUrl);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var hero = Page.Locator(".hero-card");
        var heading = hero.Locator("h1");
        var body = hero.Locator("p").First;

        var headingSize = await heading.EvaluateAsync<float>("el => parseFloat(window.getComputedStyle(el).fontSize)");
        var headingLineHeight = await heading.EvaluateAsync<float>("el => parseFloat(window.getComputedStyle(el).lineHeight)");
        var heroPaddingTop = await hero.EvaluateAsync<float>("el => parseFloat(window.getComputedStyle(el).paddingTop)");
        var heroPaddingLeft = await hero.EvaluateAsync<float>("el => parseFloat(window.getComputedStyle(el).paddingLeft)");
        var bodyLineHeight = await body.EvaluateAsync<float>("el => parseFloat(window.getComputedStyle(el).lineHeight)");

        Assert.True(headingSize >= 32, $"Expected hero heading >= 32px, got {headingSize}px.");
        Assert.True(headingLineHeight >= 36, $"Expected hero heading line-height >= 36px, got {headingLineHeight}px.");
        Assert.True(heroPaddingTop >= 20, $"Expected hero top padding >= 20px, got {heroPaddingTop}px.");
        Assert.True(heroPaddingLeft >= 20, $"Expected hero left padding >= 20px, got {heroPaddingLeft}px.");
        Assert.True(bodyLineHeight >= 22, $"Expected hero body line-height >= 22px, got {bodyLineHeight}px.");
    }

    [Fact]
    public async Task Navigation_MobileBottomBar_UsesTouchFriendlyTargets()
    {
        await Page.SetViewportSizeAsync(390, 844);
        await LoginAndGoToTripsAsync();

        var tabBar = Page.Locator(".app-tabbar");
        var bottom = await tabBar.EvaluateAsync<string>("el => window.getComputedStyle(el).bottom");
        var position = await tabBar.EvaluateAsync<string>("el => window.getComputedStyle(el).position");

        Assert.Equal("fixed", position);
        Assert.Equal("0px", bottom);

        var tabs = Page.Locator(".app-tabbar .tab-link");
        var tabCount = await tabs.CountAsync();
        Assert.Equal(3, tabCount);

        for (var i = 0; i < tabCount; i++)
        {
            var box = await tabs.Nth(i).BoundingBoxAsync();
            Assert.NotNull(box);
            Assert.True(box.Width >= 44, $"Tab {i + 1} width is {box.Width}px, expected >= 44px.");
            Assert.True(box.Height >= 44, $"Tab {i + 1} height is {box.Height}px, expected >= 44px.");
        }
    }

    [Fact]
    public async Task Navigation_DesktopLeftRail_IsPersistentAndComfortable()
    {
        await Page.SetViewportSizeAsync(1280, 900);
        await LoginAndGoToTripsAsync();

        var tabBar = Page.Locator(".app-tabbar");
        var top = await tabBar.EvaluateAsync<string>("el => window.getComputedStyle(el).top");
        var width = await tabBar.EvaluateAsync<float>("el => parseFloat(window.getComputedStyle(el).width)");
        var borderRight = await tabBar.EvaluateAsync<string>("el => window.getComputedStyle(el).borderRightStyle");

        Assert.Equal("56px", top);
        Assert.True(width >= 80 && width <= 120, $"Expected desktop nav rail width 80-120px, got {width}px.");
        Assert.Equal("solid", borderRight);
    }

    [Fact]
    public async Task TripsCards_ModernVisualHierarchy_MaintainsPaletteUsage()
    {
        await LoginAndGoToTripsAsync();
        if (await Page.Locator(".trip-item").CountAsync() == 0)
        {
            await CreateTripAsync($"Design Trip {Guid.NewGuid().ToString("N")[..8]}", "Design acceptance content");
        }

        var card = Page.Locator(".trip-item").First;
        var cardRadius = await card.EvaluateAsync<float>("el => parseFloat(window.getComputedStyle(el).borderTopLeftRadius)");
        var cardBorderWidth = await card.EvaluateAsync<float>("el => parseFloat(window.getComputedStyle(el).borderTopWidth)");
        var linkPadding = await card.Locator(".trip-main-link").EvaluateAsync<float>("el => parseFloat(window.getComputedStyle(el).paddingTop)");
        var chipBackground = await card.Locator(".trip-date-chip").EvaluateAsync<string>("el => window.getComputedStyle(el).backgroundColor");

        Assert.True(cardRadius >= 12, $"Expected trip card radius >= 12px, got {cardRadius}px.");
        Assert.True(cardBorderWidth >= 1, $"Expected trip card visible border >= 1px, got {cardBorderWidth}px.");
        Assert.True(linkPadding >= 12, $"Expected trip card inner padding >= 12px, got {linkPadding}px.");
        Assert.Equal("rgb(216, 167, 72)", chipBackground);
    }

    [Fact]
    public async Task PrimaryPages_MobileViewport_HaveNoHorizontalOverflow()
    {
        await Page.SetViewportSizeAsync(390, 844);
        await LoginAsync();

        var urls = new[] { "/", "/Trips", "/Account/Profile" };
        foreach (var relativeUrl in urls)
        {
            await Page.GotoAsync($"{AppUrl}{relativeUrl}");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            var dimensions = await Page.EvaluateAsync<int[]>(
                "() => [document.documentElement.scrollWidth, document.documentElement.clientWidth]");

            Assert.True(dimensions[0] <= dimensions[1] + 1,
                $"Expected no horizontal overflow on '{relativeUrl}', but scrollWidth={dimensions[0]} and clientWidth={dimensions[1]}.");
        }
    }
}
