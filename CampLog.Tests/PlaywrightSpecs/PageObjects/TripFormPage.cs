using Microsoft.Playwright;

namespace CampLog.Tests.PlaywrightSpecs.PageObjects;

public class TripFormPage : BasePage
{
    public TripFormPage(IPage page, string baseUrl) : base(page, baseUrl) { }

    private ILocator Container => Page.Locator("#trip-form-container");

    public ILocator NameInput           => Container.Locator("input[name='Input.Name']");
    public ILocator DescriptionTextarea => Container.Locator("textarea[name='Input.Description']");
    public ILocator StartDateInput      => Container.Locator("input[name='Input.StartDate']");
    public ILocator EndDateInput        => Container.Locator("input[name='Input.EndDate']");
    public ILocator LatitudeInput       => Container.Locator("input[name='Input.Latitude']");
    public ILocator LongitudeInput      => Container.Locator("input[name='Input.Longitude']");
    public ILocator SubmitButton        => Container.Locator("[type=submit]");
    public ILocator CancelButton        => Container.Locator("button[data-close-sheet]");
    public ILocator ValidationSummary   => Container.Locator(".validation-summary-errors, [data-valmsg-summary]");

    public async Task WaitForFormAsync() =>
        await NameInput.WaitForAsync(new LocatorWaitForOptions { Timeout = 5_000 });

    public async Task FillNameAsync(string name)            => await NameInput.FillAsync(name);
    public async Task FillDescriptionAsync(string desc)     => await DescriptionTextarea.FillAsync(desc);
    public async Task FillStartDateAsync(string date)       => await StartDateInput.FillAsync(date);
    public async Task FillEndDateAsync(string date)         => await EndDateInput.FillAsync(date);

    public async Task SubmitAsync()
    {
        await SubmitButton.ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    public async Task CancelAsync() => await CancelButton.ClickAsync();

    public async Task<string> GetNameValueAsync()        => await NameInput.InputValueAsync();
    public async Task<string> GetDescriptionValueAsync() => await DescriptionTextarea.InputValueAsync();
    public async Task<string> GetStartDateValueAsync()   => await StartDateInput.InputValueAsync();
}
