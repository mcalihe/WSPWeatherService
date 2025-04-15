namespace WSPWeatherService.Application.Interfaces;

public interface IWeatherDataFetcher
{
    Task FetchAndStoreAsync(DateTimeOffset? start = null, DateTimeOffset? end = null,
        CancellationToken cancellationToken = default);
}