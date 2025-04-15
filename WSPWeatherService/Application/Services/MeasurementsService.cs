using Microsoft.EntityFrameworkCore;
using WSPWeatherService.Api.Models;
using WSPWeatherService.Application.Interfaces;
using WSPWeatherService.Persistence;
using WSPWeatherService.Persistence.Models;

namespace WSPWeatherService.Application.Services;

/// <summary>
///     Provides methods for querying and aggregating historical weather measurement data from the database.
/// </summary>
public class MeasurementsService : IMeasurementsService
{
    private readonly WeatherDbContext _db;

    public MeasurementsService(WeatherDbContext db)
    {
        _db = db;
    }

    /// <summary>
    ///     Retrieves all measurements that match the specified query.
    /// </summary>
    /// <param name="query">The filter criteria including type, date range, and station (optional).</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>An array of matching measurements, ordered by timestamp descending.</returns>
    public async Task<MeasurementDto[]> GetAllAsync(MeasurementQuery query, CancellationToken ct = default)
    {
        return (await ApplyQuery(query).ToArrayAsync(ct))
            .Select(m => m.ToDto())
            .OrderByDescending(m => m.Timestamp)
            .ToArray();
    }

    /// <summary>
    ///     Retrieves the measurement with the maximum value for the specified type, date range, station, and unit.
    /// </summary>
    /// <param name="query">The aggregation query including measurement type, time range, station, and unit.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>The measurement with the highest value or null if no data matches.</returns>
    public async Task<MeasurementDto?> GetMaxAsync(MeasurementAggregationQuery query, CancellationToken ct = default)
    {
        return (await ApplyQuery(query)
                .OrderByDescending(m => m.Value)
                .FirstOrDefaultAsync(ct))
            .ToDto();
    }

    /// <summary>
    ///     Retrieves the measurement with the minimum value for the specified type, date range, station, and unit.
    /// </summary>
    /// <param name="query">The aggregation query including measurement type, time range, station, and unit.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>The measurement with the lowest value or null if no data matches.</returns>
    public async Task<MeasurementDto?> GetMinAsync(MeasurementAggregationQuery query, CancellationToken ct = default)
    {
        return (await ApplyQuery(query)
                .OrderBy(m => m.Value)
                .FirstOrDefaultAsync(ct))
            .ToDto();
    }

    /// <summary>
    ///     Calculates the average value for the specified measurement type, time range, station, and unit.
    /// </summary>
    /// <param name="query">The aggregation query.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>The average value, or null if no data matches.</returns>
    public async Task<double?> GetAverageAsync(MeasurementAggregationQuery query, CancellationToken ct = default)
    {
        var values = ApplyQuery(query).Select(m => m.Value);

        if (!await values.AnyAsync(ct))
            return null;

        return await values.AverageAsync(ct);
    }

    /// <summary>
    ///     Counts the number of measurements that match the given criteria.
    /// </summary>
    /// <param name="query">The aggregation query.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>The count of matching measurements.</returns>
    public async Task<int> GetCountAsync(MeasurementAggregationQuery query, CancellationToken ct = default)
    {
        return await ApplyQuery(query).CountAsync(ct);
    }

    /// <summary>
    ///     Returns a distinct, ordered list of all stations available in the database.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>An array of station names.</returns>
    public async Task<string[]> GetStations(CancellationToken cancellationToken = default)
    {
        return await _db.Measurements
            .Select(m => m.Station)
            .Distinct()
            .OrderBy(m => m)
            .ToArrayAsync(cancellationToken);
    }

    /// <summary>
    ///     Returns a distinct, ordered list of all units used for a given measurement type.
    /// </summary>
    /// <param name="type">The measurement type.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>An array of units (e.g., °C, %, hPa).</returns>
    public async Task<string[]> GetUnits(MeasurementType type, CancellationToken cancellationToken = default)
    {
        return await _db.Measurements
            .Where(m => m.Type == type)
            .Select(m => m.Unit)
            .Distinct()
            .OrderBy(u => u)
            .ToArrayAsync(cancellationToken);
    }

    /// <summary>
    ///     Builds a filtered query over the measurements table based on the specified criteria.
    /// </summary>
    /// <param name="measurementQuery">The filter criteria.</param>
    /// <returns>An <see cref="IQueryable{MeasurementEntity}" /> for deferred execution.</returns>
    internal IQueryable<MeasurementEntity> ApplyQuery(MeasurementQuery measurementQuery)
    {
        var query = _db.Measurements
            .Where(m => m.Type == measurementQuery.Type)
            .Where(m => m.Timestamp >= measurementQuery.Start && m.Timestamp <= measurementQuery.End);

        if (!string.IsNullOrWhiteSpace(measurementQuery.Station))
        {
            query = query.Where(m => m.Station == measurementQuery.Station);
        }

        if (measurementQuery is MeasurementAggregationQuery measurementAggregationQuery)
        {
            query = query.Where(m => m.Unit == measurementAggregationQuery.Unit);
        }

        return query;
    }
}