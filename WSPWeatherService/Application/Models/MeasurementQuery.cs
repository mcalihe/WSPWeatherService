using WSPWeatherService.Persistence.Models;

namespace WSPWeatherService.Application.Models;

public record MeasurementQuery(
    MeasurementType Type,
    DateTimeOffset Start,
    DateTimeOffset End,
    string? Station
);