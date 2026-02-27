using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CampLog.Web.Pages.Trips;

[Authorize]
public class DeleteModel(IHttpClientFactory httpClientFactory) : PageModel
{
    [BindProperty]
    public TripDto? Trip { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var client = await CreateApiClientAsync();
        Trip = await client.GetFromJsonAsync<TripDto>($"/trips/{id}");
        if (Trip is null)
        {
            return NotFound();
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(Guid id)
    {
        var client = await CreateApiClientAsync();
        await client.DeleteAsync($"/trips/{id}");
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
