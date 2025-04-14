using Microsoft.EntityFrameworkCore;
using Tecdottir.WeatherClient;
using WSPWeatherService.Persistence;
using WSPWeatherService.Persistence.Models;

namespace WSPWeatherService;

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

    public async Task FetchAndStoreAsync(CancellationToken cancellationToken = default)
    {
        var end = DateTimeOffset.Now.Date;
        var start = end.AddDays(-1);

        _logger.LogInformation("Fetching weather data...");
        var fetchedData = await FetchAsync(start, end, cancellationToken);
        _logger.LogInformation("Weather data fetch completed.");

        _logger.LogInformation("Start mapping fetched weather data...");
        var measurements = MapToMeasurementEntities(fetchedData);
        _logger.LogInformation("Mapping completed. Count: {count}", measurements.Length);

        _logger.LogInformation("Start filtering already existing entries...");
        var newMeasurements = await FilterAlreadyExistingEntriesAsync(measurements, start, end, cancellationToken);
        _logger.LogInformation("Filtering completed. Count: {count}", newMeasurements.Length);

        _logger.LogInformation("Inserting new measurement entities into DB...");
        await _dbContext.Measurements.AddRangeAsync(newMeasurements, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("FetchAndStoreAsync completed!");
    }

    private async Task<MeasurementEntity[]> FilterAlreadyExistingEntriesAsync(
        MeasurementEntity[] measurements,
        DateTime start, DateTime end,
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

    private async Task<MeasurementResponse[]> FetchAsync(
        DateTime start, DateTime end,
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
                        "Failed to fetch weather data for {Station}! Response did not indicate a success status",
                        station);
                    continue;
                }

                results.AddRange(response.Result);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Failed to fetch weather data for {Station}! The following exception has occured:", station);
            }
        }

        return results.ToArray();
    }

    private static MeasurementEntity[] MapToMeasurementEntities(MeasurementResponse[] measurementResponses)
    {
        var entities = new List<MeasurementEntity>();

        foreach (var measurementResponse in measurementResponses)
        {
            if (measurementResponse.Values.Air_temperature.Status == Air_temperatureStatus.Ok
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

            if (measurementResponse.Values.Water_temperature.Status == Water_temperatureStatus.Ok
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

            if (measurementResponse.Values.Barometric_pressure_qfe.Status == Barometric_pressure_qfeStatus.Ok
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

            if (measurementResponse.Values.Humidity.Status == HumidityStatus.Ok
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

    private static (string Station, DateTimeOffset Timestamp, MeasurementType Type) GetUniqueKey(
        MeasurementEntity m)
    {
        return (m.Station, m.Timestamp, m.Type);
    }
}