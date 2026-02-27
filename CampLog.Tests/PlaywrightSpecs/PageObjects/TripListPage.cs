using Microsoft.Playwright;

namespace CampLog.Tests.PlaywrightSpecs.PageObjects;

public class TripListPage : BasePage
{
    public TripListPage(IPage page, string baseUrl) : base(page, baseUrl) { }

    public ILocator PageHeading  => Page.Locator("h1");
    public ILocator EmptyState   => Page.Locator(".trip-empty-state");
    public ILocator TripList     => Page.Locator(".trip-list");
    public ILocator TripItems    => Page.Locator(".trip-item");
    public ILocator FabButton    => Page.Locator(".fab-add-trip");
    public ILocator EditButtons  => Page.Locator(".muted-action:has-text('Edit')");
    public ILocator DeleteButtons => Page.Locator(".muted-action:has-text('Delete')");

    public async Task<TripListPage> GotoAsync()
    {
        await Page.GotoAsync($"{BaseUrl}/Trips");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        return this;
    }

    public async Task OpenCreateFormAsync()
    {
        await FabButton.ClickAsync();
        await Page.WaitForFunctionAsync(
            "document.body.classList.contains('sheet-open')",
            null,
            new PageWaitForFunctionOptions { Timeout = 5_000 });
    }

    public async Task ClickEditFirstTripAsync()
    {
        await EditButtons.First.ClickAsync();
        await Page.WaitForFunctionAsync(
            "document.body.classList.contains('sheet-open')",
            null,
            new PageWaitForFunctionOptions { Timeout = 5_000 });
    }

    public async Task<int> GetTripCountAsync()        => await TripItems.CountAsync();
    public async Task<bool> IsEmptyStateVisibleAsync() => await EmptyState.IsVisibleAsync();
    public async Task<bool> IsFabVisibleAsync()        => await FabButton.IsVisibleAsync();
}
