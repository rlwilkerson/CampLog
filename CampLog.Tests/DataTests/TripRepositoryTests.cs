using CampLog.Api.Data;
using CampLog.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CampLog.Tests.DataTests;

public class TripRepositoryTests
{
    private static CampLogDbContext CreateContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<CampLogDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new CampLogDbContext(options);
    }

    [Fact]
    public async Task GetByUserIdAsync_ReturnsOnlyThatUsersTrips()
    {
        var db = CreateContext(nameof(GetByUserIdAsync_ReturnsOnlyThatUsersTrips));
        var userId1 = Guid.NewGuid();
        var userId2 = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;

        db.Trips.AddRange(
            new Trip { Id = Guid.NewGuid(), UserId = userId1, Name = "Trip A", CreatedAt = now, UpdatedAt = now },
            new Trip { Id = Guid.NewGuid(), UserId = userId1, Name = "Trip B", CreatedAt = now, UpdatedAt = now },
            new Trip { Id = Guid.NewGuid(), UserId = userId2, Name = "Trip C", CreatedAt = now, UpdatedAt = now }
        );
        await db.SaveChangesAsync();

        var repo = new TripRepository(db);
        var result = (await repo.GetByUserIdAsync(userId1)).ToList();

        Assert.Equal(2, result.Count);
        Assert.All(result, t => Assert.Equal(userId1, t.UserId));
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsCorrectTrip()
    {
        var db = CreateContext(nameof(GetByIdAsync_ReturnsCorrectTrip));
        var tripId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;

        db.Trips.Add(new Trip { Id = tripId, UserId = Guid.NewGuid(), Name = "My Trip", CreatedAt = now, UpdatedAt = now });
        await db.SaveChangesAsync();

        var repo = new TripRepository(db);
        var result = await repo.GetByIdAsync(tripId);

        Assert.NotNull(result);
        Assert.Equal(tripId, result.Id);
        Assert.Equal("My Trip", result.Name);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
    {
        var db = CreateContext(nameof(GetByIdAsync_ReturnsNull_WhenNotFound));
        var repo = new TripRepository(db);

        var result = await repo.GetByIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_PersistsTrip()
    {
        var db = CreateContext(nameof(CreateAsync_PersistsTrip));
        var now = DateTimeOffset.UtcNow;
        var trip = new Trip { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), Name = "New Trip", CreatedAt = now, UpdatedAt = now };

        var repo = new TripRepository(db);
        var created = await repo.CreateAsync(trip);

        Assert.Equal(trip.Id, created.Id);
        Assert.Equal(1, await db.Trips.CountAsync());
    }

    [Fact]
    public async Task UpdateAsync_UpdatesFields()
    {
        var db = CreateContext(nameof(UpdateAsync_UpdatesFields));
        var userId = Guid.NewGuid();
        var tripId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;

        db.Trips.Add(new Trip { Id = tripId, UserId = userId, Name = "Old Name", Description = "Old desc", CreatedAt = now, UpdatedAt = now });
        await db.SaveChangesAsync();

        var repo = new TripRepository(db);
        var updated = await repo.UpdateAsync(new Trip
        {
            Id = tripId,
            UserId = userId,
            Name = "New Name",
            Description = "New desc",
            UpdatedAt = DateTimeOffset.UtcNow
        });

        Assert.NotNull(updated);
        Assert.Equal("New Name", updated.Name);
        Assert.Equal("New desc", updated.Description);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsNull_WhenTripNotFound()
    {
        var db = CreateContext(nameof(UpdateAsync_ReturnsNull_WhenTripNotFound));
        var repo = new TripRepository(db);

        var result = await repo.UpdateAsync(new Trip { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), Name = "X", UpdatedAt = DateTimeOffset.UtcNow });

        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteAsync_RemovesTrip()
    {
        var db = CreateContext(nameof(DeleteAsync_RemovesTrip));
        var userId = Guid.NewGuid();
        var tripId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;

        db.Trips.Add(new Trip { Id = tripId, UserId = userId, Name = "Trip", CreatedAt = now, UpdatedAt = now });
        await db.SaveChangesAsync();

        var repo = new TripRepository(db);
        var result = await repo.DeleteAsync(tripId, userId);

        Assert.True(result);
        Assert.Equal(0, await db.Trips.CountAsync());
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalse_WhenWrongUserId()
    {
        var db = CreateContext(nameof(DeleteAsync_ReturnsFalse_WhenWrongUserId));
        var userId = Guid.NewGuid();
        var tripId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;

        db.Trips.Add(new Trip { Id = tripId, UserId = userId, Name = "Trip", CreatedAt = now, UpdatedAt = now });
        await db.SaveChangesAsync();

        var repo = new TripRepository(db);
        var result = await repo.DeleteAsync(tripId, Guid.NewGuid());

        Assert.False(result);
        Assert.Equal(1, await db.Trips.CountAsync());
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalse_WhenTripNotFound()
    {
        var db = CreateContext(nameof(DeleteAsync_ReturnsFalse_WhenTripNotFound));
        var repo = new TripRepository(db);

        var result = await repo.DeleteAsync(Guid.NewGuid(), Guid.NewGuid());

        Assert.False(result);
    }
}
