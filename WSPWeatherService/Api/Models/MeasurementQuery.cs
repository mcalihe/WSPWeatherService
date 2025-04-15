using WSPWeatherService.Persistence.Models;

namespace WSPWeatherService.Api.Models;

public record MeasurementQuery(
    MeasurementType Type,
    DateTimeOffset Start,
    DateTimeOffset End,
    string? Station
);