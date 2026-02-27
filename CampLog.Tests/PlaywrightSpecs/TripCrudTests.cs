using CampLog.Tests.PlaywrightSpecs.PageObjects;
using Microsoft.Playwright;

namespace CampLog.Tests.PlaywrightSpecs;

/// <summary>
/// Trip CRUD E2E tests. Covers list, empty state, FAB, slide-up sheet, create, edit, delete.
/// Run with: dotnet test --filter "Category=PlaywrightUI"
/// </summary>
[Trait("Category", "PlaywrightUI")]
public class TripCrudTests : PlaywrightSetup
{
    // -----------------------------------------------------------------------
    // Trips list — layout and empty state
    // -----------------------------------------------------------------------

    [Fact]
    public async Task TripsPage_ShowsMyTripsHeading()
    {
        await LoginAndGoToTripsAsync();
        var heading = await Page.Locator("h1").TextContentAsync();
        Assert.Equal("My Trips", heading?.Trim());
    }

    [Fact]
    public async Task TripsPage_FABButton_IsVisible()
    {
        await LoginAndGoToTripsAsync();
        Assert.True(await Page.Locator(".fab-add-trip").IsVisibleAsync());
    }

    [Fact]
    public async Task TripsPage_FABButton_HasFixedPosition()
    {
        await LoginAndGoToTripsAsync();
        var position = await Page.Locator(".fab-add-trip")
            .EvaluateAsync<string>("el => window.getComputedStyle(el).position");
        Assert.Equal("fixed", position);
    }

    [Fact]
    public async Task TripsPage_WhenNoTrips_EmptyStateShowsCorrectMessage()
    {
        await LoginAndGoToTripsAsync();
        var hasEmptyState = await Page.Locator(".trip-empty-state").IsVisibleAsync();
        var hasTripList   = await Page.Locator(".trip-list").IsVisibleAsync();

        // Exactly one of empty-state or trip-list should be visible
        Assert.NotEqual(hasEmptyState, hasTripList);
        if (hasEmptyState)
        {
            Assert.True(await Page.Locator(".trip-empty-state p:has-text('No trips yet')").IsVisibleAsync());
            Assert.True(await Page.Locator(".trip-empty-state small").IsVisibleAsync());
        }
    }

    // -----------------------------------------------------------------------
    // Trip card rendering
    // -----------------------------------------------------------------------

    [Fact]
    public async Task TripCard_ShowsName_DateChip_Description_AndChevron()
    {
        await LoginAndGoToTripsAsync();

        var tripName = await CreateTripAsync("Card Render Test", "Trip description for card test", "2025-08-15");
        await Page.GotoAsync($"{AppUrl}/Trips");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var card = Page.Locator(".trip-item", new PageLocatorOptions { HasText = tripName });

        Assert.True(await card.Locator("h3").IsVisibleAsync());
        Assert.Contains(tripName, await card.Locator("h3").TextContentAsync() ?? "");

        Assert.True(await card.Locator(".trip-date-chip").IsVisibleAsync());
        var dateChipText = await card.Locator(".trip-date-chip").TextContentAsync();
        Assert.NotEmpty(dateChipText ?? "");

        // Description snippet
        Assert.True(await card.Locator(".trip-snippet").IsVisibleAsync());

        // Chevron
        Assert.True(await card.Locator(".trip-chevron").IsVisibleAsync());
    }

    [Fact]
    public async Task TripCard_NoDateTrip_ShowsNoDateFallback()
    {
        await LoginAndGoToTripsAsync();

        // Create trip without a start date
        await OpenCreateSheetAsync();
        var noDateName = $"No Date Trip {Guid.NewGuid():N[..6]}";
        await Page.FillAsync("#trip-form-container input[name='Input.Name']", noDateName);
        await Page.Locator("#trip-form-container [type=submit]").ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var card = Page.Locator(".trip-item", new PageLocatorOptions { HasText = noDateName });
        if (await card.CountAsync() > 0)
        {
            var dateChip = card.Locator(".trip-date-chip");
            var chipText = await dateChip.TextContentAsync();
            Assert.Contains("No date", chipText ?? "");
        }
    }

    // -----------------------------------------------------------------------
    // FAB / slide-up sheet
    // -----------------------------------------------------------------------

    [Fact]
    public async Task FAB_Click_OpensSlideUpSheet()
    {
        await LoginAndGoToTripsAsync();

        await Page.Locator(".fab-add-trip").ClickAsync();
        await Page.WaitForFunctionAsync(
            "document.body.classList.contains('sheet-open')",
            null, new PageWaitForFunctionOptions { Timeout = 5_000 });

        Assert.True(await Page.EvaluateAsync<bool>("() => document.body.classList.contains('sheet-open')"));
        Assert.False(await Page.Locator("#sheet-panel").EvaluateAsync<bool>("el => el.hidden"));
    }

    [Fact]
    public async Task CreateForm_ContainsAllRequiredFields()
    {
        await LoginAndGoToTripsAsync();
        await OpenCreateSheetAsync();

        var form = new TripFormPage(Page, AppUrl);
        Assert.True(await form.NameInput.IsVisibleAsync());
        Assert.True(await form.DescriptionTextarea.IsVisibleAsync());
        Assert.True(await form.StartDateInput.IsVisibleAsync());
        Assert.True(await form.EndDateInput.IsVisibleAsync());
        Assert.True(await form.SubmitButton.IsVisibleAsync());
        Assert.True(await form.CancelButton.IsVisibleAsync());
    }

    [Fact]
    public async Task CreateForm_SheetCloseButton_DismissesWithoutSaving()
    {
        await LoginAndGoToTripsAsync();
        var initialCount = await Page.Locator(".trip-item").CountAsync();

        await OpenCreateSheetAsync();
        await Page.FillAsync("#trip-form-container input[name='Input.Name']", "Should Not Be Saved");
        await Page.Locator(".sheet-close").ClickAsync();

        await Page.WaitForFunctionAsync(
            "!document.body.classList.contains('sheet-open')",
            null, new PageWaitForFunctionOptions { Timeout = 3_000 });

        var afterCount = await Page.Locator(".trip-item").CountAsync();
        Assert.Equal(initialCount, afterCount);
        Assert.False(await Page.Locator("text=Should Not Be Saved").IsVisibleAsync());
    }

    [Fact]
    public async Task CreateForm_CancelButton_DismissesSheet()
    {
        await LoginAndGoToTripsAsync();
        await OpenCreateSheetAsync();

        var form = new TripFormPage(Page, AppUrl);
        await form.CancelAsync();

        await Page.WaitForFunctionAsync(
            "!document.body.classList.contains('sheet-open')",
            null, new PageWaitForFunctionOptions { Timeout = 3_000 });

        Assert.False(await Page.EvaluateAsync<bool>("() => document.body.classList.contains('sheet-open')"));
    }

    [Fact]
    public async Task CreateForm_EscapeKey_DismissesSheet()
    {
        await LoginAndGoToTripsAsync();
        await OpenCreateSheetAsync();

        await Page.Keyboard.PressAsync("Escape");
        await Page.WaitForFunctionAsync(
            "!document.body.classList.contains('sheet-open')",
            null, new PageWaitForFunctionOptions { Timeout = 3_000 });

        Assert.False(await Page.EvaluateAsync<bool>("() => document.body.classList.contains('sheet-open')"));
    }

    [Fact]
    public async Task CreateForm_OverlayClick_DismissesSheet()
    {
        await LoginAndGoToTripsAsync();
        await OpenCreateSheetAsync();

        await Page.Locator("#sheet-overlay").ClickAsync(new LocatorClickOptions { Force = true });
        await Page.WaitForFunctionAsync(
            "!document.body.classList.contains('sheet-open')",
            null, new PageWaitForFunctionOptions { Timeout = 3_000 });

        Assert.False(await Page.EvaluateAsync<bool>("() => document.body.classList.contains('sheet-open')"));
    }

    // -----------------------------------------------------------------------
    // Create trip
    // -----------------------------------------------------------------------

    [Fact]
    public async Task CreateTrip_ValidInput_TripAppearsInList()
    {
        await LoginAndGoToTripsAsync();

        var tripName = $"E2E Create {Guid.NewGuid():N[..8]}";
        await OpenCreateSheetAsync();

        var form = new TripFormPage(Page, AppUrl);
        await form.FillNameAsync(tripName);
        await form.FillDescriptionAsync("An automated test trip");
        await form.FillStartDateAsync("2025-07-04");
        await form.FillEndDateAsync("2025-07-10");
        await form.SubmitAsync();

        Assert.True(await Page.Locator($"text={tripName}").IsVisibleAsync());
    }

    [Fact]
    public async Task CreateTrip_AfterSubmit_SheetIsClosed()
    {
        await LoginAndGoToTripsAsync();
        await OpenCreateSheetAsync();

        var form = new TripFormPage(Page, AppUrl);
        await form.FillNameAsync($"Sheet Close Test {Guid.NewGuid():N[..6]}");
        await form.SubmitAsync();

        // Sheet should be dismissed after successful create
        Assert.False(await Page.EvaluateAsync<bool>("() => document.body.classList.contains('sheet-open')"));
    }

    [Fact]
    public async Task CreateTrip_IncreasesListCount()
    {
        await LoginAndGoToTripsAsync();
        var countBefore = await Page.Locator(".trip-item").CountAsync();

        await CreateTripAsync($"Count Test {Guid.NewGuid():N[..6]}");

        var countAfter = await Page.Locator(".trip-item").CountAsync();
        Assert.Equal(countBefore + 1, countAfter);
    }

    // -----------------------------------------------------------------------
    // Edit trip
    // -----------------------------------------------------------------------

    [Fact]
    public async Task EditTrip_Click_OpensSheetWithPrePopulatedName()
    {
        await LoginAndGoToTripsAsync();

        // Ensure at least one trip exists
        if (await Page.Locator(".trip-item").CountAsync() == 0)
            await CreateTripAsync("Pre-populate Test Trip");

        await Page.Locator(".muted-action:has-text('Edit')").First.ClickAsync();
        await Page.WaitForFunctionAsync(
            "document.body.classList.contains('sheet-open')",
            null, new PageWaitForFunctionOptions { Timeout = 5_000 });

        var form = new TripFormPage(Page, AppUrl);
        await form.WaitForFormAsync();

        var nameValue = await form.GetNameValueAsync();
        Assert.NotEmpty(nameValue);
    }

    [Fact]
    public async Task EditTrip_SaveChanges_UpdatesNameInList()
    {
        await LoginAndGoToTripsAsync();

        if (await Page.Locator(".trip-item").CountAsync() == 0)
            await CreateTripAsync("Pre-Edit Trip");

        await Page.Locator(".muted-action:has-text('Edit')").First.ClickAsync();
        await Page.WaitForFunctionAsync(
            "document.body.classList.contains('sheet-open')",
            null, new PageWaitForFunctionOptions { Timeout = 5_000 });

        var form = new TripFormPage(Page, AppUrl);
        await form.WaitForFormAsync();

        var updatedName = $"Updated {Guid.NewGuid():N[..8]}";
        await form.NameInput.ClearAsync();
        await form.FillNameAsync(updatedName);
        await form.SubmitAsync();

        Assert.True(await Page.Locator($"text={updatedName}").IsVisibleAsync());
    }

    [Fact]
    public async Task EditTrip_FormContainsSaveButton()
    {
        await LoginAndGoToTripsAsync();

        if (await Page.Locator(".trip-item").CountAsync() == 0)
            await CreateTripAsync("Edit Button Test Trip");

        await Page.Locator(".muted-action:has-text('Edit')").First.ClickAsync();
        await Page.WaitForFunctionAsync(
            "document.body.classList.contains('sheet-open')",
            null, new PageWaitForFunctionOptions { Timeout = 5_000 });

        var saveBtn = Page.Locator("#trip-form-container [type=submit]");
        await saveBtn.WaitForAsync(new LocatorWaitForOptions { Timeout = 5_000 });

        var btnText = await saveBtn.TextContentAsync();
        Assert.Equal("Save", btnText?.Trim());
    }

    // -----------------------------------------------------------------------
    // Delete trip
    // -----------------------------------------------------------------------

    [Fact]
    public async Task DeleteTrip_RemovesTripFromList()
    {
        await LoginAndGoToTripsAsync();

        // Ensure there is a trip to delete
        if (await Page.Locator(".trip-item").CountAsync() == 0)
            await CreateTripAsync("Trip To Delete");

        var countBefore = await Page.Locator(".trip-item").CountAsync();

        await Page.Locator(".muted-action:has-text('Delete')").First.ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // The delete page may show a confirmation form — submit it if present
        var confirmSubmit = Page.Locator("[type=submit]");
        if (await confirmSubmit.IsVisibleAsync())
        {
            await confirmSubmit.ClickAsync();
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        }

        var countAfter = await Page.Locator(".trip-item").CountAsync();
        Assert.True(countAfter < countBefore);
    }

    [Fact]
    public async Task DeleteTrip_AfterDeletion_ReturnsToTripsList()
    {
        await LoginAndGoToTripsAsync();

        if (await Page.Locator(".trip-item").CountAsync() == 0)
            await CreateTripAsync("Trip For Delete Return Test");

        await Page.Locator(".muted-action:has-text('Delete')").First.ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var confirmSubmit = Page.Locator("[type=submit]");
        if (await confirmSubmit.IsVisibleAsync())
        {
            await confirmSubmit.ClickAsync();
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        }

        Assert.Contains($"{AppUrl}/Trips", Page.Url);
    }
}
