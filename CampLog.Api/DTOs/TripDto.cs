namespace CampLog.Api.DTOs;

public record TripDto(
    Guid Id,
    string Name,
    string? Description,
    double? Latitude,
    double? Longitude,
    DateOnly? StartDate,
    DateOnly? EndDate
);
