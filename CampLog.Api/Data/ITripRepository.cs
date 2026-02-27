using CampLog.Api.Models;

namespace CampLog.Api.Data;

public interface ITripRepository
{
    Task<IEnumerable<Trip>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<Trip?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Trip> CreateAsync(Trip trip, CancellationToken ct = default);
    Task<Trip?> UpdateAsync(Trip trip, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, Guid userId, CancellationToken ct = default);
}
