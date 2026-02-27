using CampLog.Api.Models;
using System.Security.Claims;

namespace CampLog.Api.Services;

public interface IUserService
{
    Task<User> GetOrCreateAsync(ClaimsPrincipal claims, CancellationToken ct = default);
}
