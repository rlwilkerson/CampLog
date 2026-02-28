using System.Security.Claims;

namespace CampLog.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static string GetKeycloakId(this ClaimsPrincipal user) =>
        user.FindFirstValue("sub")
        ?? user.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new InvalidOperationException("No sub claim");

    public static string GetEmail(this ClaimsPrincipal user) =>
        user.FindFirstValue("email") ?? string.Empty;

    public static string GetDisplayName(this ClaimsPrincipal user) =>
        user.FindFirstValue("preferred_username") ?? string.Empty;
}
