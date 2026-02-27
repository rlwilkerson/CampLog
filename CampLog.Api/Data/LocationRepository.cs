using CampLog.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CampLog.Api.Data;

public class LocationRepository(CampLogDbContext dbContext) : ILocationRepository
{
    public async Task<IEnumerable<Location>> GetByTripIdAsync(Guid tripId, CancellationToken ct = default)
    {
        return await dbContext.Locations
            .Where(l => l.TripId == tripId)
            .AsNoTracking()
            .ToListAsync(ct);
    }

    public async Task<Location?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await dbContext.Locations
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == id, ct);
    }

    public async Task<Location> CreateAsync(Location location, CancellationToken ct = default)
    {
        dbContext.Locations.Add(location);
        await dbContext.SaveChangesAsync(ct);
        return location;
    }

    public async Task<Location?> UpdateAsync(Location location, CancellationToken ct = default)
    {
        var existing = await dbContext.Locations.FirstOrDefaultAsync(l => l.Id == location.Id && l.TripId == location.TripId, ct);
        if (existing is null)
        {
            return null;
        }

        existing.Name = location.Name;
        existing.Description = location.Description;
        existing.Latitude = location.Latitude;
        existing.Longitude = location.Longitude;
        existing.StartDate = location.StartDate;
        existing.EndDate = location.EndDate;
        existing.UpdatedAt = location.UpdatedAt;

        await dbContext.SaveChangesAsync(ct);
        return existing;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tripId, CancellationToken ct = default)
    {
        var existing = await dbContext.Locations.FirstOrDefaultAsync(l => l.Id == id && l.TripId == tripId, ct);
        if (existing is null)
        {
            return false;
        }

        dbContext.Locations.Remove(existing);
        await dbContext.SaveChangesAsync(ct);
        return true;
    }
}
