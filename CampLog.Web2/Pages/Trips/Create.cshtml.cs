using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CampLog.Web.Pages.Trips;

[Authorize]
public class CreateModel(IHttpClientFactory httpClientFactory) : PageModel
{
    [BindProperty]
    public TripUpsertRequest Input { get; set; } = new();

    public bool IsHtmxRequest { get; private set; }

    public void OnGet()
    {
        IsHtmxRequest = Request.Headers.ContainsKey("HX-Request");
    }

    public async Task<IActionResult> OnPostAsync()
    {
        IsHtmxRequest = Request.Headers.ContainsKey("HX-Request");
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var client = await CreateApiClientAsync();
        var response = await client.PostAsJsonAsync("/trips", Input);
        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError(string.Empty, "Unable to create trip.");
            return Page();
        }

        return RedirectToPage("/Trips/Index");
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
