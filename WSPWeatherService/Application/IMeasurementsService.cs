using WSPWeatherService.Application.Models;

namespace WSPWeatherService.Application;

public interface IMeasurementsService
{
    Task<MeasurementDto[]> GetAllAsync(MeasurementQuery query, CancellationToken cancellationToken = default);
    Task<MeasurementDto?> GetMaxAsync(MeasurementAggregationQuery query, CancellationToken cancellationToken = default);
    Task<MeasurementDto?> GetMinAsync(MeasurementAggregationQuery query, CancellationToken cancellationToken = default);
    Task<double?> GetAverageAsync(MeasurementAggregationQuery query, CancellationToken cancellationToken = default);
    Task<int> GetCountAsync(MeasurementAggregationQuery query, CancellationToken cancellationToken = default);
}