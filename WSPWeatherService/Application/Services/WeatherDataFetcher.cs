using Microsoft.EntityFrameworkCore;
using Tecdottir.WeatherClient;
using WSPWeatherService.Application.Interfaces;
using WSPWeatherService.Persistence;
using WSPWeatherService.Persistence.Models;

namespace WSPWeatherService.Application.Services;

/// <summary>
///     Handles the fetching, mapping, deduplication, and storage of weather data from the Zürich Water Police API.
/// </summary>
public class WeatherDataFetcher : IWeatherDataFetcher
{
    private const int FetchLimit = 100;
    private readonly WeatherDbContext _dbContext;
    private readonly ILogger<WeatherDataFetcher> _logger;
    private readonly Station2[] _stationsToFetch = [Station2.Mythenquai, Station2.Tiefenbrunnen];
    private readonly WeatherClient _weatherClient;

    public WeatherDataFetcher(ILogger<WeatherDataFetcher> logger, WeatherDbContext dbContext,
        WeatherClient weatherClient)
    {
        _logger = logger;
        _dbContext = dbContext;
        _weatherClient = weatherClient;
    }

    /// <summary>
    ///     Fetches weather data for the specified time range, maps it, filters out existing entries,
    ///     and inserts new data into the database.
    /// </summary>
    /// <param name="start">Start of the time range (defaults to yesterday).</param>
    /// <param name="end">End of the time range (defaults to 1 day after start).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task FetchAndStoreAsync(DateTimeOffset? start = null, DateTimeOffset? end = null,
        CancellationToken cancellationToken = default)
    {
        if (start > end)
        {
            throw new ArgumentException("Start date must be before end date", nameof(start));
        }

        end ??= DateTimeOffset.Now.Date;
        start ??= end.Value.AddDays(-1);

        _logger.LogInformation("Fetching weather data...");
        var fetchedData = await FetchAsync(start.Value, end.Value, cancellationToken);
        _logger.LogInformation("Weather data fetch completed.");

        _logger.LogInformation("Start mapping fetched weather data...");
        var measurements = MapToMeasurementEntities(fetchedData);
        _logger.LogInformation("Mapping completed. Count: {count}", measurements.Length);

        _logger.LogInformation("Start filtering already existing entries...");
        var newMeasurements =
            await FilterAlreadyExistingEntriesAsync(measurements, start.Value, end.Value, cancellationToken);
        _logger.LogInformation("Filtering completed. Count: {count}", newMeasurements.Length);

        _logger.LogInformation("Inserting new measurement entities into DB...");
        await _dbContext.Measurements.AddRangeAsync(newMeasurements, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("FetchAndStoreAsync completed!");
    }

    /// <summary>
    ///     Filters out measurements that already exist in the database within the specified date range.
    /// </summary>
    /// <param name="measurements">All measurements to check.</param>
    /// <param name="start">Start of date range.</param>
    /// <param name="end">End of date range.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>New measurements that are not yet in the database.</returns>
    internal async Task<MeasurementEntity[]> FilterAlreadyExistingEntriesAsync(
        MeasurementEntity[] measurements,
        DateTimeOffset start, DateTimeOffset end,
        CancellationToken cancellationToken = default)
    {
        // This in-memory filtering approach is simple and sufficient for this exercise.
        // Further optimization (e.g. via raw SQL or batched WHERE NOT EXISTS queries) would be possible in a production setup.

        var allExisting = await _dbContext.Measurements
            .Where(m => m.Timestamp >= start && m.Timestamp <= end)
            .ToListAsync(cancellationToken);

        var existingKeys = allExisting
            .Select(GetUniqueKey).ToHashSet();

        var newMeasurements = measurements
            .Where(m => !existingKeys.Contains(GetUniqueKey(m)))
            .ToArray();

        return newMeasurements;
    }

    /// <summary>
    ///     Fetches raw weather data for the given time range from the Tecdottir API.
    /// </summary>
    /// <param name="start">Start of the range.</param>
    /// <param name="end">End of the range.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Array of raw weather measurements.</returns>
    internal async Task<MeasurementResponse[]> FetchAsync(
        DateTimeOffset start, DateTimeOffset end,
        CancellationToken cancellationToken = default)
    {
        var sort = $"{nameof(Measurement.Timestamp_cet).ToLower()} desc";
        var results = new List<MeasurementResponse>();

        foreach (var station in _stationsToFetch)
        {
            // Retry logic could be added here,
            // but is omitted for simplicity in this exercise.

            try
            {
                var response = await _weatherClient.MeasurementsAsync(station, start, end,
                    sort, FetchLimit, cancellationToken: cancellationToken);

                if (!response.Ok)
                {
                    _logger.LogWarning(
                        "Error while fetching weather data for {Station}! Response did not indicate success",
                        station);
                    continue;
                }

                results.AddRange(response.Result);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Error while fetching weather data for {Station}: {Message}", station, ex.Message);
            }
        }

        return results.ToArray();
    }

    /// <summary>
    ///     Converts raw measurement responses into database entities, only including valid and complete data.
    /// </summary>
    /// <param name="measurementResponses">The raw measurement responses from the API.</param>
    /// <returns>An array of valid <see cref="MeasurementEntity" /> objects.</returns>
    public static MeasurementEntity[] MapToMeasurementEntities(MeasurementResponse[] measurementResponses)
    {
        var entities = new List<MeasurementEntity>();

        foreach (var measurementResponse in measurementResponses)
        {
            if (measurementResponse.Values.Air_temperature is not null
                && measurementResponse.Values.Air_temperature.Status == Air_temperatureStatus.Ok
                && measurementResponse.Values.Air_temperature.Value.HasValue)
            {
                entities.Add(new MeasurementEntity
                {
                    Station = measurementResponse.Station,
                    Timestamp = measurementResponse.Timestamp,
                    Type = MeasurementType.AirTemperature,
                    Value = measurementResponse.Values.Air_temperature.Value.Value,
                    Unit = measurementResponse.Values.Air_temperature.Unit
                });
            }

            if (measurementResponse.Values.Water_temperature is not null
                && measurementResponse.Values.Water_temperature.Status == Water_temperatureStatus.Ok
                && measurementResponse.Values.Water_temperature.Value.HasValue)
            {
                entities.Add(new MeasurementEntity
                {
                    Station = measurementResponse.Station,
                    Timestamp = measurementResponse.Timestamp,
                    Type = MeasurementType.WaterTemperature,
                    Value = measurementResponse.Values.Water_temperature.Value.Value,
                    Unit = measurementResponse.Values.Water_temperature.Unit
                });
            }

            if (measurementResponse.Values.Barometric_pressure_qfe is not null
                && measurementResponse.Values.Barometric_pressure_qfe.Status == Barometric_pressure_qfeStatus.Ok
                && measurementResponse.Values.Barometric_pressure_qfe.Value.HasValue)
            {
                entities.Add(new MeasurementEntity
                {
                    Station = measurementResponse.Station,
                    Timestamp = measurementResponse.Timestamp,
                    Type = MeasurementType.AirPressure,
                    Value = measurementResponse.Values.Barometric_pressure_qfe.Value.Value,
                    Unit = measurementResponse.Values.Barometric_pressure_qfe.Unit
                });
            }

            if (measurementResponse.Values.Humidity is not null
                && measurementResponse.Values.Humidity.Status == HumidityStatus.Ok
                && measurementResponse.Values.Humidity.Value.HasValue)
            {
                entities.Add(new MeasurementEntity
                {
                    Station = measurementResponse.Station,
                    Timestamp = measurementResponse.Timestamp,
                    Type = MeasurementType.Humidity,
                    Value = measurementResponse.Values.Humidity.Value.Value,
                    Unit = measurementResponse.Values.Humidity.Unit
                });
            }
        }

        return entities.ToArray();
    }

    internal static (string Station, DateTimeOffset Timestamp, MeasurementType Type) GetUniqueKey(
        MeasurementEntity m)
    {
        return (m.Station, m.Timestamp, m.Type);
    }
}