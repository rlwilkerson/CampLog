using CampLog.Api.Extensions;
using System.Security.Claims;

namespace CampLog.Tests;

public class ClaimsPrincipalExtensionsTests
{
    [Fact]
    public void GetKeycloakId_ReturnsSubClaim_WhenPresent()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity([new Claim("sub", "user-sub")]));
        Assert.Equal("user-sub", principal.GetKeycloakId());
    }

    [Fact]
    public void GetKeycloakId_ReturnsNameIdentifier_WhenSubMapped()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, "mapped-sub")]));
        Assert.Equal("mapped-sub", principal.GetKeycloakId());
    }
}
