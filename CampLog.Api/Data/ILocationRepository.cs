using CampLog.Api.Models;

namespace CampLog.Api.Data;

public interface ILocationRepository
{
    Task<IEnumerable<Location>> GetByTripIdAsync(Guid tripId, CancellationToken ct = default);
    Task<Location?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Location> CreateAsync(Location location, CancellationToken ct = default);
    Task<Location?> UpdateAsync(Location location, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, Guid tripId, CancellationToken ct = default);
}
