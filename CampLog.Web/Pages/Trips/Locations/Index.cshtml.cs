using System.Net.Http.Headers;
using System.Net.Http.Json;
using CampLog.Web.Pages.Trips;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CampLog.Web.Pages.Trips.Locations;

[Authorize]
public class IndexModel(IHttpClientFactory httpClientFactory) : PageModel
{
    public Guid TripId { get; private set; }
    public List<LocationDto> Locations { get; private set; } = [];

    public async Task OnGetAsync(Guid tripId)
    {
        TripId = tripId;
        var client = await CreateApiClientAsync();
        Locations = await client.GetFromJsonAsync<List<LocationDto>>($"/trips/{tripId}/locations") ?? [];
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
