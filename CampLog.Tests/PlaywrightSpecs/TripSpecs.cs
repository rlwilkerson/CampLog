using Microsoft.Playwright;

namespace CampLog.Tests;

public class TripSpecs
{
    private const string AppUrl = "http://localhost:5000";

    [Fact]
    public async Task TripsPage_ShowsEmptyState_WhenNoTrips()
    {
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync();
        var page = await browser.NewPageAsync();

        await page.GotoAsync($"{AppUrl}/Trips");

        var emptyMsg = await page.Locator("text=No trips yet").IsVisibleAsync();
        Assert.True(emptyMsg);
    }

    [Fact]
    public async Task TripsPage_ShowsTrips_WhenTripsExist()
    {
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync();
        var page = await browser.NewPageAsync();

        await page.GotoAsync($"{AppUrl}/Trips");

        // Table with trip rows should be visible
        await page.WaitForSelectorAsync("table");
        var rows = await page.Locator("tbody tr").CountAsync();
        Assert.True(rows > 0);
    }

    [Fact]
    public async Task CreateTrip_FormSubmit_AddsTrip()
    {
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync();
        var page = await browser.NewPageAsync();

        await page.GotoAsync($"{AppUrl}/Trips");
        await page.ClickAsync("text=New Trip");

        // Wait for the form to load via HTMX
        await page.WaitForSelectorAsync("#trip-form-container input[name='Name']");
        await page.FillAsync("#trip-form-container input[name='Name']", "Playwright Test Trip");
        await page.ClickAsync("#trip-form-container [type=submit]");

        await page.WaitForSelectorAsync("text=Playwright Test Trip");
        Assert.True(await page.Locator("text=Playwright Test Trip").IsVisibleAsync());
    }

    [Fact]
    public async Task EditTrip_FormSubmit_UpdatesTrip()
    {
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync();
        var page = await browser.NewPageAsync();

        await page.GotoAsync($"{AppUrl}/Trips");

        // Click the first Edit link
        await page.Locator("a:text('Edit')").First.ClickAsync();
        await page.WaitForSelectorAsync("input[name='Name']");

        await page.FillAsync("input[name='Name']", "Updated Trip Name");
        await page.ClickAsync("[type=submit]");

        await page.WaitForURLAsync($"{AppUrl}/Trips");
        Assert.True(await page.Locator("text=Updated Trip Name").IsVisibleAsync());
    }

    [Fact]
    public async Task DeleteTrip_Confirm_RemovesTrip()
    {
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync();
        var page = await browser.NewPageAsync();

        await page.GotoAsync($"{AppUrl}/Trips");

        var rowsBefore = await page.Locator("tbody tr").CountAsync();

        // Click the first Delete link
        await page.Locator("a:text('Delete')").First.ClickAsync();
        await page.WaitForSelectorAsync("[type=submit]");
        await page.ClickAsync("[type=submit]");

        await page.WaitForURLAsync($"{AppUrl}/Trips");
        var rowsAfter = await page.Locator("tbody tr").CountAsync();
        Assert.True(rowsAfter < rowsBefore);
    }
}
