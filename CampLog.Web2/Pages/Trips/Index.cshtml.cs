using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CampLog.Web.Pages.Trips;

[Authorize]
public class IndexModel(IHttpClientFactory httpClientFactory) : PageModel
{
    public List<TripDto> Trips { get; private set; } = [];

    public async Task OnGet()
    {
        var client = await CreateApiClientAsync();
        Trips = await client.GetFromJsonAsync<List<TripDto>>("/trips") ?? [];
    }

    public string GetDescriptionSnippet(string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return "-";
        }

        return description.Length <= 80 ? description : $"{description[..80]}...";
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
