namespace WSPWeatherService;

public interface IWeatherDataFetcher
{
    Task FetchAndStoreAsync();
}