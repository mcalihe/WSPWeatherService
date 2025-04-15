using Microsoft.EntityFrameworkCore;
using WSPWeatherService.Api.Models;
using WSPWeatherService.Application.Interfaces;
using WSPWeatherService.Persistence;
using WSPWeatherService.Persistence.Models;

namespace WSPWeatherService.Application.Services;

public class MeasurementsService : IMeasurementsService
{
    private readonly WeatherDbContext _db;

    public MeasurementsService(WeatherDbContext db)
    {
        _db = db;
    }

    public async Task<MeasurementDto[]> GetAllAsync(MeasurementQuery query, CancellationToken ct = default)
    {
        return (await ApplyQuery(query).ToArrayAsync(ct)).Select(m => m.ToDto()).ToArray();
    }

    public async Task<MeasurementDto?> GetMaxAsync(MeasurementAggregationQuery query, CancellationToken ct = default)
    {
        return (await ApplyQuery(query).OrderByDescending(m => m.Value).FirstOrDefaultAsync(ct)).ToDto();
    }

    public async Task<MeasurementDto?> GetMinAsync(MeasurementAggregationQuery query, CancellationToken ct = default)
    {
        return (await ApplyQuery(query).OrderBy(m => m.Value).FirstOrDefaultAsync(ct)).ToDto();
    }

    public async Task<double?> GetAverageAsync(MeasurementAggregationQuery query, CancellationToken ct = default)
    {
        var values = ApplyQuery(query).Select(m => m.Value);

        if (!await values.AnyAsync(ct))
            return null;

        return await values.AverageAsync(ct);
    }

    public async Task<int> GetCountAsync(MeasurementAggregationQuery query, CancellationToken ct = default)
    {
        return await ApplyQuery(query).CountAsync(ct);
    }

    public async Task<string[]> GetStations(CancellationToken cancellationToken = default)
    {
        return await _db.Measurements
            .Select(m => m.Station)
            .Distinct()
            .ToArrayAsync(cancellationToken);
    }

    public async Task<string[]> GetUnits(MeasurementType type, CancellationToken cancellationToken = default)
    {
        return await _db.Measurements
            .Where(m => m.Type == type)
            .Select(m => m.Unit)
            .Distinct()
            .ToArrayAsync(cancellationToken);
    }

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