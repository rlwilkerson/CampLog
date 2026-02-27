using CampLog.Api.Data;
using CampLog.Api.DTOs;
using CampLog.Api.Models;
using CampLog.Api.Services;
using System.Security.Claims;

namespace CampLog.Api.Endpoints;

public static class TripEndpoints
{
    public static IEndpointRouteBuilder MapTripEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/trips").RequireAuthorization();

        group.MapGet("/", async (ITripRepository trips, IUserService users, ClaimsPrincipal claims, CancellationToken ct) =>
        {
            var user = await users.GetOrCreateAsync(claims, ct);
            var items = await trips.GetByUserIdAsync(user.Id, ct);
            return Results.Ok(items.Select(ToDto));
        });

        group.MapGet("/{id:guid}", async (Guid id, ITripRepository trips, IUserService users, ClaimsPrincipal claims, CancellationToken ct) =>
        {
            var user = await users.GetOrCreateAsync(claims, ct);
            var trip = await trips.GetByIdAsync(id, ct);
            if (trip is null)
            {
                return Results.NotFound();
            }

            return trip.UserId != user.Id ? Results.Forbid() : Results.Ok(ToDto(trip));
        });

        group.MapPost("/", async (CreateTripRequest request, ITripRepository trips, IUserService users, ClaimsPrincipal claims, CancellationToken ct) =>
        {
            var user = await users.GetOrCreateAsync(claims, ct);
            var now = DateTimeOffset.UtcNow;
            var created = await trips.CreateAsync(new Trip
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Name = request.Name,
                Description = request.Description,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                CreatedAt = now,
                UpdatedAt = now
            }, ct);

            return Results.Created($"/trips/{created.Id}", ToDto(created));
        });

        group.MapPut("/{id:guid}", async (Guid id, UpdateTripRequest request, ITripRepository trips, IUserService users, ClaimsPrincipal claims, CancellationToken ct) =>
        {
            var user = await users.GetOrCreateAsync(claims, ct);
            var existing = await trips.GetByIdAsync(id, ct);
            if (existing is null)
            {
                return Results.NotFound();
            }

            if (existing.UserId != user.Id)
            {
                return Results.Forbid();
            }

            existing.Name = request.Name;
            existing.Description = request.Description;
            existing.Latitude = request.Latitude;
            existing.Longitude = request.Longitude;
            existing.StartDate = request.StartDate;
            existing.EndDate = request.EndDate;
            existing.UpdatedAt = DateTimeOffset.UtcNow;

            var updated = await trips.UpdateAsync(existing, ct);
            return updated is null ? Results.NotFound() : Results.Ok(ToDto(updated));
        });

        group.MapDelete("/{id:guid}", async (Guid id, ITripRepository trips, IUserService users, ClaimsPrincipal claims, CancellationToken ct) =>
        {
            var user = await users.GetOrCreateAsync(claims, ct);
            var existing = await trips.GetByIdAsync(id, ct);
            if (existing is null)
            {
                return Results.NotFound();
            }

            if (existing.UserId != user.Id)
            {
                return Results.Forbid();
            }

            await trips.DeleteAsync(id, user.Id, ct);
            return Results.NoContent();
        });

        return app;
    }

    private static TripDto ToDto(Trip trip) =>
        new(trip.Id, trip.Name, trip.Description, trip.Latitude, trip.Longitude, trip.StartDate, trip.EndDate);
}
