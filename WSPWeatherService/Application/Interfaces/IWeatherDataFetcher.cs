namespace WSPWeatherService.Application.Interfaces;

public interface IWeatherDataFetcher
{
    Task FetchAndStoreAsync(CancellationToken cancellationToken = default);
}