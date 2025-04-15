using WSPWeatherService.Persistence.Models;

namespace WSPWeatherService.Api.Models;

public class MeasurementDto
{
    public Guid Id { get; set; }
    public required string Station { get; set; }
    public DateTimeOffset Timestamp { get; set; }
    public MeasurementType Type { get; set; }
    public double Value { get; set; }
    public required string Unit { get; set; }
}