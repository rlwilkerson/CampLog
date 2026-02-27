using Microsoft.Playwright;

namespace CampLog.Tests.PlaywrightSpecs.PageObjects;

/// <summary>Base page object: top bar, tab bar, and common layout elements.</summary>
public abstract class BasePage
{
    protected readonly IPage Page;
    protected readonly string BaseUrl;

    protected BasePage(IPage page, string baseUrl)
    {
        Page = page;
        BaseUrl = baseUrl;
    }

    // Top bar
    public ILocator TopBar        => Page.Locator(".app-topbar");
    public ILocator BrandLink     => Page.Locator(".brand-link");
    public ILocator UserAvatar    => Page.Locator(".user-avatar");
    public ILocator LoginIconLink => Page.Locator(".login-icon-link");

    // Tab bar
    public ILocator TabBar      => Page.Locator(".app-tabbar");
    public ILocator HomeTab     => Page.Locator(".app-tabbar a[href='/']");
    public ILocator TripsTab    => Page.Locator(".app-tabbar a[href='/Trips']");
    public ILocator ProfileTab  => Page.Locator(".app-tabbar a[href='/Account/Login']");

    // Sheet
    public ILocator SheetOverlay    => Page.Locator("#sheet-overlay");
    public ILocator SheetPanel      => Page.Locator("#sheet-panel");
    public ILocator SheetCloseBtn   => Page.Locator(".sheet-close");
    public ILocator TripFormContainer => Page.Locator("#trip-form-container");

    public async Task<bool> IsTabBarVisibleAsync() => await TabBar.IsVisibleAsync();

    public async Task<bool> IsTabActiveAsync(ILocator tab) =>
        await tab.EvaluateAsync<bool>("el => el.classList.contains('is-active')");

    public async Task<bool> IsSheetOpenAsync() =>
        await Page.EvaluateAsync<bool>("() => document.body.classList.contains('sheet-open')");
}
