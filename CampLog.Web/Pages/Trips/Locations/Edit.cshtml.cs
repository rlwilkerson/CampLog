using System.Net.Http.Headers;
using System.Net.Http.Json;
using CampLog.Web.Pages.Trips;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CampLog.Web.Pages.Trips.Locations;

[Authorize]
public class EditModel(IHttpClientFactory httpClientFactory) : PageModel
{
    [BindProperty]
    public EditLocationInput Input { get; set; } = new();

    public Guid TripId { get; private set; }

    public async Task<IActionResult> OnGetAsync(Guid tripId, Guid id)
    {
        TripId = tripId;
        var client = await CreateApiClientAsync();
        var location = await client.GetFromJsonAsync<LocationDto>($"/trips/{tripId}/locations/{id}");
        if (location is null)
        {
            return NotFound();
        }

        Input = new EditLocationInput
        {
            Id = location.Id,
            Name = location.Name,
            Description = location.Description,
            Latitude = location.Latitude,
            Longitude = location.Longitude,
            StartDate = location.StartDate,
            EndDate = location.EndDate
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(Guid tripId)
    {
        TripId = tripId;
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var client = await CreateApiClientAsync();
        var request = new LocationUpsertRequest
        {
            Name = Input.Name,
            Description = Input.Description,
            Latitude = Input.Latitude,
            Longitude = Input.Longitude,
            StartDate = Input.StartDate,
            EndDate = Input.EndDate
        };

        var response = await client.PutAsJsonAsync($"/trips/{tripId}/locations/{Input.Id}", request);
        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError(string.Empty, "Unable to update location.");
            return Page();
        }

        return RedirectToPage("/Trips/Locations/Index", new { tripId });
    }

    public class EditLocationInput : LocationUpsertRequest
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
