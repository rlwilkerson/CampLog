using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CampLog.Web.Pages.Account;

[AllowAnonymous]
public class RegisterModel(IConfiguration configuration) : PageModel
{
    public string RegistrationUrl { get; private set; } = string.Empty;

    public IActionResult OnGet()
    {
        var keycloakUrl = configuration["services:keycloak:http:0"] ?? "http://localhost:8080";
        RegistrationUrl = $"{keycloakUrl}/realms/camplog/protocol/openid-connect/registrations?client_id=camplog-web&response_type=code&redirect_uri=/";
        return Redirect(RegistrationUrl);
    }
}
