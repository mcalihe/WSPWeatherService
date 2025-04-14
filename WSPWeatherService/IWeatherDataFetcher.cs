namespace WSPWeatherService;

public interface IWeatherDataFetcher
{
    Task FetchAndStoreAsync(CancellationToken cancellationToken = default);
}