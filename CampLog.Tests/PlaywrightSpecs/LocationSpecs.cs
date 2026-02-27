using Microsoft.Playwright;

namespace CampLog.Tests;

public class LocationSpecs
{
    private const string AppUrl = "http://localhost:5000";

    [Fact]
    public async Task LocationsPage_ShowsLocations_ForTrip()
    {
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync();
        var page = await browser.NewPageAsync();

        // Navigate to a trip's locations page (requires a known tripId in the running app)
        await page.GotoAsync($"{AppUrl}/Trips");
        await page.Locator("a:text('Locations')").First.ClickAsync();

        await page.WaitForSelectorAsync("h1");
        var heading = await page.Locator("h1").TextContentAsync();
        Assert.NotNull(heading);
    }

    [Fact]
    public async Task CreateLocation_FormSubmit_AddsLocation()
    {
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync();
        var page = await browser.NewPageAsync();

        await page.GotoAsync($"{AppUrl}/Trips");
        await page.Locator("a:text('Locations')").First.ClickAsync();

        await page.ClickAsync("text=New Location");
        await page.WaitForSelectorAsync("input[name='Name']");
        await page.FillAsync("input[name='Name']", "Playwright Test Location");
        await page.ClickAsync("[type=submit]");

        await page.WaitForSelectorAsync("text=Playwright Test Location");
        Assert.True(await page.Locator("text=Playwright Test Location").IsVisibleAsync());
    }

    [Fact]
    public async Task EditLocation_FormSubmit_UpdatesLocation()
    {
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync();
        var page = await browser.NewPageAsync();

        await page.GotoAsync($"{AppUrl}/Trips");
        await page.Locator("a:text('Locations')").First.ClickAsync();

        await page.Locator("a:text('Edit')").First.ClickAsync();
        await page.WaitForSelectorAsync("input[name='Name']");

        await page.FillAsync("input[name='Name']", "Updated Location Name");
        await page.ClickAsync("[type=submit]");

        Assert.True(await page.Locator("text=Updated Location Name").IsVisibleAsync());
    }

    [Fact]
    public async Task DeleteLocation_Confirm_RemovesLocation()
    {
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync();
        var page = await browser.NewPageAsync();

        await page.GotoAsync($"{AppUrl}/Trips");
        await page.Locator("a:text('Locations')").First.ClickAsync();

        var rowsBefore = await page.Locator("tbody tr").CountAsync();

        await page.Locator("a:text('Delete')").First.ClickAsync();
        await page.WaitForSelectorAsync("[type=submit]");
        await page.ClickAsync("[type=submit]");

        var rowsAfter = await page.Locator("tbody tr").CountAsync();
        Assert.True(rowsAfter < rowsBefore);
    }
}
