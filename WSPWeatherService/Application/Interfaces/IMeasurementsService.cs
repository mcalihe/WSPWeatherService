using WSPWeatherService.Api.Models;
using WSPWeatherService.Persistence.Models;

namespace WSPWeatherService.Application.Interfaces;

/// <summary>
///     Provides methods for querying stored weather measurement data.
/// </summary>
public interface IMeasurementsService
{
    /// <summary>
    ///     Retrieves all measurements matching the given query.
    /// </summary>
    /// <param name="query">The filter criteria including time range, type, and optional station.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An array of matching measurements sorted by descending timestamp.</returns>
    Task<MeasurementDto[]> GetAllAsync(MeasurementQuery query, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Retrieves the measurement with the highest value for the specified type, unit, and time range.
    /// </summary>
    /// <param name="query">The aggregation query including type, unit, time range, and optional station.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The measurement with the maximum value, or null if none found.</returns>
    Task<MeasurementDto?> GetMaxAsync(MeasurementAggregationQuery query, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Retrieves the measurement with the lowest value for the specified type, unit, and time range.
    /// </summary>
    /// <param name="query">The aggregation query including type, unit, time range, and optional station.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The measurement with the minimum value, or null if none found.</returns>
    Task<MeasurementDto?> GetMinAsync(MeasurementAggregationQuery query, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Calculates the average value for the specified type, unit, and time range.
    /// </summary>
    /// <param name="query">The aggregation query including type, unit, time range, and optional station.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The average value, or null if no matching data exists.</returns>
    Task<double?> GetAverageAsync(MeasurementAggregationQuery query, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Counts the number of stored measurements for the specified type, unit, and time range.
    /// </summary>
    /// <param name="query">The aggregation query including type, unit, time range, and optional station.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The number of matching entries.</returns>
    Task<int> GetCountAsync(MeasurementAggregationQuery query, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Retrieves the distinct list of stations currently available in the database.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An array of station names.</returns>
    Task<string[]> GetStations(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Retrieves the distinct list of units used for the specified measurement type.
    /// </summary>
    /// <param name="type">The measurement type.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An array of units.</returns>
    Task<string[]> GetUnits(MeasurementType type, CancellationToken cancellationToken = default);
}