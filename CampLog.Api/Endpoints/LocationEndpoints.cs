using CampLog.Api.Data;
using CampLog.Api.DTOs;
using CampLog.Api.Models;
using CampLog.Api.Services;
using System.Security.Claims;

namespace CampLog.Api.Endpoints;

public static class LocationEndpoints
{
    public static IEndpointRouteBuilder MapLocationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/trips/{tripId:guid}/locations").RequireAuthorization();

        group.MapGet("/", async (Guid tripId, ILocationRepository locations, ITripRepository trips, IUserService users, ClaimsPrincipal claims, CancellationToken ct) =>
        {
            var user = await users.GetOrCreateAsync(claims, ct);
            var trip = await trips.GetByIdAsync(tripId, ct);
            if (trip is null)
            {
                return Results.NotFound();
            }

            if (trip.UserId != user.Id)
            {
                return Results.Forbid();
            }

            var items = await locations.GetByTripIdAsync(tripId, ct);
            return Results.Ok(items.Select(ToDto));
        });

        group.MapGet("/{id:guid}", async (Guid tripId, Guid id, ILocationRepository locations, ITripRepository trips, IUserService users, ClaimsPrincipal claims, CancellationToken ct) =>
        {
            var user = await users.GetOrCreateAsync(claims, ct);
            var trip = await trips.GetByIdAsync(tripId, ct);
            if (trip is null)
            {
                return Results.NotFound();
            }

            if (trip.UserId != user.Id)
            {
                return Results.Forbid();
            }

            var location = await locations.GetByIdAsync(id, ct);
            if (location is null || location.TripId != tripId)
            {
                return Results.NotFound();
            }

            return Results.Ok(ToDto(location));
        });

        group.MapPost("/", async (Guid tripId, CreateLocationRequest request, ILocationRepository locations, ITripRepository trips, IUserService users, ClaimsPrincipal claims, CancellationToken ct) =>
        {
            var user = await users.GetOrCreateAsync(claims, ct);
            var trip = await trips.GetByIdAsync(tripId, ct);
            if (trip is null)
            {
                return Results.NotFound();
            }

            if (trip.UserId != user.Id)
            {
                return Results.Forbid();
            }

            var now = DateTimeOffset.UtcNow;
            var created = await locations.CreateAsync(new Location
            {
                Id = Guid.NewGuid(),
                TripId = tripId,
                Name = request.Name,
                Description = request.Description,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                CreatedAt = now,
                UpdatedAt = now
            }, ct);

            return Results.Created($"/trips/{tripId}/locations/{created.Id}", ToDto(created));
        });

        group.MapPut("/{id:guid}", async (Guid tripId, Guid id, UpdateLocationRequest request, ILocationRepository locations, ITripRepository trips, IUserService users, ClaimsPrincipal claims, CancellationToken ct) =>
        {
            var user = await users.GetOrCreateAsync(claims, ct);
            var trip = await trips.GetByIdAsync(tripId, ct);
            if (trip is null)
            {
                return Results.NotFound();
            }

            if (trip.UserId != user.Id)
            {
                return Results.Forbid();
            }

            var existing = await locations.GetByIdAsync(id, ct);
            if (existing is null || existing.TripId != tripId)
            {
                return Results.NotFound();
            }

            existing.Name = request.Name;
            existing.Description = request.Description;
            existing.Latitude = request.Latitude;
            existing.Longitude = request.Longitude;
            existing.StartDate = request.StartDate;
            existing.EndDate = request.EndDate;
            existing.UpdatedAt = DateTimeOffset.UtcNow;

            var updated = await locations.UpdateAsync(existing, ct);
            return updated is null ? Results.NotFound() : Results.Ok(ToDto(updated));
        });

        group.MapDelete("/{id:guid}", async (Guid tripId, Guid id, ILocationRepository locations, ITripRepository trips, IUserService users, ClaimsPrincipal claims, CancellationToken ct) =>
        {
            var user = await users.GetOrCreateAsync(claims, ct);
            var trip = await trips.GetByIdAsync(tripId, ct);
            if (trip is null)
            {
                return Results.NotFound();
            }

            if (trip.UserId != user.Id)
            {
                return Results.Forbid();
            }

            var deleted = await locations.DeleteAsync(id, tripId, ct);
            return deleted ? Results.NoContent() : Results.NotFound();
        });

        return app;
    }

    private static LocationDto ToDto(Location location) =>
        new(location.Id, location.TripId, location.Name, location.Description, location.Latitude, location.Longitude, location.StartDate, location.EndDate);
}
