using CampLog.Api.Data;
using CampLog.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CampLog.Tests.DataTests;

public class LocationRepositoryTests
{
    private static CampLogDbContext CreateContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<CampLogDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new CampLogDbContext(options);
    }

    [Fact]
    public async Task GetByTripIdAsync_ReturnsLocationsForTrip()
    {
        var db = CreateContext(nameof(GetByTripIdAsync_ReturnsLocationsForTrip));
        var tripId1 = Guid.NewGuid();
        var tripId2 = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;

        db.Locations.AddRange(
            new Location { Id = Guid.NewGuid(), TripId = tripId1, Name = "Loc A", CreatedAt = now, UpdatedAt = now },
            new Location { Id = Guid.NewGuid(), TripId = tripId1, Name = "Loc B", CreatedAt = now, UpdatedAt = now },
            new Location { Id = Guid.NewGuid(), TripId = tripId2, Name = "Loc C", CreatedAt = now, UpdatedAt = now }
        );
        await db.SaveChangesAsync();

        var repo = new LocationRepository(db);
        var result = (await repo.GetByTripIdAsync(tripId1)).ToList();

        Assert.Equal(2, result.Count);
        Assert.All(result, l => Assert.Equal(tripId1, l.TripId));
    }

    [Fact]
    public async Task GetByTripIdAsync_ReturnsEmpty_WhenNoLocations()
    {
        var db = CreateContext(nameof(GetByTripIdAsync_ReturnsEmpty_WhenNoLocations));
        var repo = new LocationRepository(db);

        var result = await repo.GetByTripIdAsync(Guid.NewGuid());

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsCorrectLocation()
    {
        var db = CreateContext(nameof(GetByIdAsync_ReturnsCorrectLocation));
        var locationId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;

        db.Locations.Add(new Location { Id = locationId, TripId = Guid.NewGuid(), Name = "Camp Site", CreatedAt = now, UpdatedAt = now });
        await db.SaveChangesAsync();

        var repo = new LocationRepository(db);
        var result = await repo.GetByIdAsync(locationId);

        Assert.NotNull(result);
        Assert.Equal(locationId, result.Id);
        Assert.Equal("Camp Site", result.Name);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
    {
        var db = CreateContext(nameof(GetByIdAsync_ReturnsNull_WhenNotFound));
        var repo = new LocationRepository(db);

        var result = await repo.GetByIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_PersistsLocation()
    {
        var db = CreateContext(nameof(CreateAsync_PersistsLocation));
        var now = DateTimeOffset.UtcNow;
        var location = new Location { Id = Guid.NewGuid(), TripId = Guid.NewGuid(), Name = "New Site", CreatedAt = now, UpdatedAt = now };

        var repo = new LocationRepository(db);
        var created = await repo.CreateAsync(location);

        Assert.Equal(location.Id, created.Id);
        Assert.Equal(1, await db.Locations.CountAsync());
    }

    [Fact]
    public async Task UpdateAsync_UpdatesFields()
    {
        var db = CreateContext(nameof(UpdateAsync_UpdatesFields));
        var tripId = Guid.NewGuid();
        var locationId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;

        db.Locations.Add(new Location { Id = locationId, TripId = tripId, Name = "Old Name", Description = "Old", CreatedAt = now, UpdatedAt = now });
        await db.SaveChangesAsync();

        var repo = new LocationRepository(db);
        var updated = await repo.UpdateAsync(new Location
        {
            Id = locationId,
            TripId = tripId,
            Name = "New Name",
            Description = "New desc",
            UpdatedAt = DateTimeOffset.UtcNow
        });

        Assert.NotNull(updated);
        Assert.Equal("New Name", updated.Name);
        Assert.Equal("New desc", updated.Description);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsNull_WhenLocationNotFound()
    {
        var db = CreateContext(nameof(UpdateAsync_ReturnsNull_WhenLocationNotFound));
        var repo = new LocationRepository(db);

        var result = await repo.UpdateAsync(new Location { Id = Guid.NewGuid(), TripId = Guid.NewGuid(), Name = "X", UpdatedAt = DateTimeOffset.UtcNow });

        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteAsync_RemovesLocation()
    {
        var db = CreateContext(nameof(DeleteAsync_RemovesLocation));
        var tripId = Guid.NewGuid();
        var locationId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;

        db.Locations.Add(new Location { Id = locationId, TripId = tripId, Name = "Site", CreatedAt = now, UpdatedAt = now });
        await db.SaveChangesAsync();

        var repo = new LocationRepository(db);
        var result = await repo.DeleteAsync(locationId, tripId);

        Assert.True(result);
        Assert.Equal(0, await db.Locations.CountAsync());
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalse_WhenWrongTripId()
    {
        var db = CreateContext(nameof(DeleteAsync_ReturnsFalse_WhenWrongTripId));
        var tripId = Guid.NewGuid();
        var locationId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;

        db.Locations.Add(new Location { Id = locationId, TripId = tripId, Name = "Site", CreatedAt = now, UpdatedAt = now });
        await db.SaveChangesAsync();

        var repo = new LocationRepository(db);
        var result = await repo.DeleteAsync(locationId, Guid.NewGuid());

        Assert.False(result);
        Assert.Equal(1, await db.Locations.CountAsync());
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalse_WhenLocationNotFound()
    {
        var db = CreateContext(nameof(DeleteAsync_ReturnsFalse_WhenLocationNotFound));
        var repo = new LocationRepository(db);

        var result = await repo.DeleteAsync(Guid.NewGuid(), Guid.NewGuid());

        Assert.False(result);
    }
}
