using WSPWeatherService.Persistence.Models;

namespace WSPWeatherService.Api.Models;

public record MeasurementAggregationQuery(
    MeasurementType Type,
    DateTimeOffset Start,
    DateTimeOffset End,
    string? Station,
    string Unit) : MeasurementQuery(Type, Start, End, Station);