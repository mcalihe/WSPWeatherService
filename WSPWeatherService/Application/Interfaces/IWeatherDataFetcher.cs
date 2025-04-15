namespace WSPWeatherService.Application.Interfaces;

/// <summary>
///     Defines a service that fetches and stores weather data from external sources.
/// </summary>
public interface IWeatherDataFetcher
{
    /// <summary>
    ///     Fetches weather data for the specified date range and stores new entries in the database.
    ///     If no range is provided, the previous day is used by default.
    /// </summary>
    /// <param name="start">Optional start of the range.</param>
    /// <param name="end">Optional end of the range.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task FetchAndStoreAsync(DateTimeOffset? start = null, DateTimeOffset? end = null,
        CancellationToken cancellationToken = default);
}