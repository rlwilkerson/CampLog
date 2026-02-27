using System.Net.Http.Headers;
using System.Net.Http.Json;
using CampLog.Web.Pages.Trips;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CampLog.Web.Pages.Trips.Locations;

[Authorize]
public class DeleteModel(IHttpClientFactory httpClientFactory) : PageModel
{
    [BindProperty]
    public LocationDto? Location { get; set; }

    public Guid TripId { get; private set; }

    public async Task<IActionResult> OnGetAsync(Guid tripId, Guid id)
    {
        TripId = tripId;
        var client = await CreateApiClientAsync();
        Location = await client.GetFromJsonAsync<LocationDto>($"/trips/{tripId}/locations/{id}");
        if (Location is null)
        {
            return NotFound();
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(Guid tripId, Guid id)
    {
        var client = await CreateApiClientAsync();
        await client.DeleteAsync($"/trips/{tripId}/locations/{id}");
        return RedirectToPage("/Trips/Locations/Index", new { tripId });
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
