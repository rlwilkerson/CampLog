using CampLog.Api.Data;
using CampLog.Api.Extensions;
using CampLog.Api.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CampLog.Api.Services;

public class UserService(CampLogDbContext dbContext) : IUserService
{
    public async Task<User> GetOrCreateAsync(ClaimsPrincipal claims, CancellationToken ct = default)
    {
        var keycloakId = claims.GetKeycloakId();
        var existing = await dbContext.Users.FirstOrDefaultAsync(u => u.KeycloakId == keycloakId, ct);
        if (existing is not null)
        {
            return existing;
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            KeycloakId = keycloakId,
            Email = claims.GetEmail(),
            DisplayName = claims.GetDisplayName(),
            CreatedAt = DateTimeOffset.UtcNow
        };

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(ct);
        return user;
    }
}
