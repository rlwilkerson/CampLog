using System.Net.Http.Headers;
using System.Net.Http.Json;
using CampLog.Web.Pages.Trips;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CampLog.Web.Pages.Trips.Locations;

[Authorize]
public class CreateModel(IHttpClientFactory httpClientFactory) : PageModel
{
    [BindProperty]
    public LocationUpsertRequest Input { get; set; } = new();

    public Guid TripId { get; private set; }

    public void OnGet(Guid tripId)
    {
        TripId = tripId;
    }

    public async Task<IActionResult> OnPostAsync(Guid tripId)
    {
        TripId = tripId;
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var client = await CreateApiClientAsync();
        var response = await client.PostAsJsonAsync($"/trips/{tripId}/locations", Input);
        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError(string.Empty, "Unable to create location.");
            return Page();
        }

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
