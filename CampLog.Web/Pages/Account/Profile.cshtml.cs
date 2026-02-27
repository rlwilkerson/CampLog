using System.Security.Claims;
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
        if (User.Identity?.IsAuthenticated != true)
        {
            return RedirectToPage("/Account/Login");
        }

        DisplayName = User.FindFirstValue("name")
            ?? User.Identity?.Name
            ?? User.FindFirstValue(ClaimTypes.Name)
            ?? "Camper";
        Email = User.FindFirstValue("email")
            ?? User.FindFirstValue(ClaimTypes.Email)
            ?? "No email available";

        return Page();
    }
}
