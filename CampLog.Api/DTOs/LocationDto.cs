namespace CampLog.Api.DTOs;

public record LocationDto(
    Guid Id,
    Guid TripId,
    string Name,
    string? Description,
    double? Latitude,
    double? Longitude,
    DateOnly? StartDate,
    DateOnly? EndDate
);
