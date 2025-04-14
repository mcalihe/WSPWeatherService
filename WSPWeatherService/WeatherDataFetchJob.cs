namespace WSPWeatherService;

public class WeatherDataFetchJob
{
    private readonly IWeatherDataFetcher _fetcher;
    private readonly ILogger<WeatherDataFetchJob> _logger;

    public WeatherDataFetchJob(IWeatherDataFetcher fetcher, ILogger<WeatherDataFetchJob> logger)
    {
        _fetcher = fetcher;
        _logger = logger;
    }

    public async Task<bool> ExecuteAsync()
    {
        _logger.LogInformation("Starting WeatherDataFetchJob...");
        await _fetcher.FetchAndStoreAsync();
        _logger.LogInformation("WeatherDataFetchJob completed.");

        return true;
    }
}