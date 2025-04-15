using WSPWeatherService.Persistence.Models;

namespace WSPWeatherService.Api.Models;

public record MeasurementDto(
    Guid Id,
    string Station,
    DateTimeOffset Timestamp,
    MeasurementType Type,
    double Value,
    string Unit
);