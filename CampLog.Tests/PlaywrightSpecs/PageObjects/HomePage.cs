using Microsoft.Playwright;

namespace CampLog.Tests.PlaywrightSpecs.PageObjects;

public class HomePage : BasePage
{
    public HomePage(IPage page, string baseUrl) : base(page, baseUrl) { }

    public ILocator HeroCard       => Page.Locator(".hero-card");
    public ILocator Heading        => Page.Locator("h1");
    public ILocator ViewTripsLink  => Page.Locator("a[role='button']", new PageLocatorOptions { HasText = "View Trips" });
    public ILocator PrivacyLink    => Page.Locator("a.secondary[role='button']", new PageLocatorOptions { HasText = "Privacy" });

    public async Task<HomePage> GotoAsync()
    {
        await Page.GotoAsync(BaseUrl);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        return this;
    }
}
