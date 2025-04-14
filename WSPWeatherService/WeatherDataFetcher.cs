using WSPWeatherService.Persistence;

namespace WSPWeatherService;

public class WeatherDataFetcher : IWeatherDataFetcher
{
    private readonly WeatherDbContext _dbContext;
    private readonly ILogger<WeatherDataFetcher> _logger;

    public WeatherDataFetcher(ILogger<WeatherDataFetcher> logger, WeatherDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task FetchAndStoreAsync()
    {
        _logger.LogInformation("Fetching weather data...");


        // Fetch weather data from external API (via HttpClient or generated client)
        // Filter, map, persist (ensure no duplicates)
        // Example pseudo-code:
        // var data = await _httpClient.GetFromJsonAsync<YourDto>(url);
        // var validData = data.Where(x => x.IsValid());
        // _dbContext.WeatherMeasurements.AddRange(...);
        // await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Weather data fetch completed.");
    }
}