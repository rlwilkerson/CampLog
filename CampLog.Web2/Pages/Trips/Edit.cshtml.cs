using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CampLog.Web.Pages.Trips;

[Authorize]
public class EditModel(IHttpClientFactory httpClientFactory) : PageModel
{
    [BindProperty]
    public EditTripInput Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var client = await CreateApiClientAsync();
        var trip = await client.GetFromJsonAsync<TripDto>($"/trips/{id}");
        if (trip is null)
        {
            return NotFound();
        }

        Input = new EditTripInput
        {
            Id = trip.Id,
            Name = trip.Name,
            Description = trip.Description,
            Latitude = trip.Latitude,
            Longitude = trip.Longitude,
            StartDate = trip.StartDate,
            EndDate = trip.EndDate
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var client = await CreateApiClientAsync();
        var request = new TripUpsertRequest
        {
            Name = Input.Name,
            Description = Input.Description,
            Latitude = Input.Latitude,
            Longitude = Input.Longitude,
            StartDate = Input.StartDate,
            EndDate = Input.EndDate
        };

        var response = await client.PutAsJsonAsync($"/trips/{Input.Id}", request);
        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError(string.Empty, "Unable to update trip.");
            return Page();
        }

        return RedirectToPage("/Trips/Index");
    }

    public class EditTripInput : TripUpsertRequest
    {
        public Guid Id { get; set; }
    }

    private async Task<HttpClient> CreateApiClientAsync()
    {
        var client = httpClientFactory.CreateClient("api");
        var token = await GetAccessTokenAsync();
        if (!string.IsNullOrWhiteSpace(token))
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return client;
    }

    private async Task<string?> GetAccessTokenAsync() =>
        await HttpContext.GetTokenAsync("access_token");
}
