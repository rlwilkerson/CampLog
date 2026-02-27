using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CampLog.Web.Pages.Account;

[Authorize]
public class ProfileModel : PageModel
{
    public string DisplayName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;

    public IActionResult OnGet()
    {
        DisplayName = User.FindFirstValue("name")
            ?? User.Identity?.Name
            ?? User.FindFirstValue(ClaimTypes.Name)
            ?? "Camper";
        Email = User.FindFirstValue("email")
            ?? User.FindFirstValue(ClaimTypes.Email)
            ?? "No email available";

        return Page();
    }

    public IActionResult OnPost()
    {
        return SignOut(
            new AuthenticationProperties { RedirectUri = "/" },
            CookieAuthenticationDefaults.AuthenticationScheme,
            OpenIdConnectDefaults.AuthenticationScheme);
    }
}
