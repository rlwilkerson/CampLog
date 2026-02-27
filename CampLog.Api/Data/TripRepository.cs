using CampLog.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CampLog.Api.Data;

public class TripRepository(CampLogDbContext dbContext) : ITripRepository
{
    public async Task<IEnumerable<Trip>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        return await dbContext.Trips
            .Where(t => t.UserId == userId)
            .Include(t => t.Locations)
            .AsNoTracking()
            .ToListAsync(ct);
    }

    public async Task<Trip?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await dbContext.Trips
            .Include(t => t.Locations)
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id, ct);
    }

    public async Task<Trip> CreateAsync(Trip trip, CancellationToken ct = default)
    {
        dbContext.Trips.Add(trip);
        await dbContext.SaveChangesAsync(ct);
        return trip;
    }

    public async Task<Trip?> UpdateAsync(Trip trip, CancellationToken ct = default)
    {
        var existing = await dbContext.Trips.FirstOrDefaultAsync(t => t.Id == trip.Id && t.UserId == trip.UserId, ct);
        if (existing is null)
        {
            return null;
        }

        existing.Name = trip.Name;
        existing.Description = trip.Description;
        existing.Latitude = trip.Latitude;
        existing.Longitude = trip.Longitude;
        existing.StartDate = trip.StartDate;
        existing.EndDate = trip.EndDate;
        existing.UpdatedAt = trip.UpdatedAt;

        await dbContext.SaveChangesAsync(ct);
        return existing;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid userId, CancellationToken ct = default)
    {
        var existing = await dbContext.Trips.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId, ct);
        if (existing is null)
        {
            return false;
        }

        dbContext.Trips.Remove(existing);
        await dbContext.SaveChangesAsync(ct);
        return true;
    }
}
