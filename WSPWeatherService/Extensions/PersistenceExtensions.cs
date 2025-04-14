using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using WSPWeatherService.Options;
using WSPWeatherService.Persistence;

namespace WSPWeatherService.Extensions;

public static class PersistenceExtensions
{
    public static IServiceCollection AddWeatherDbContext(this IServiceCollection services)
    {
        services.AddDbContext<WeatherDbContext>((sp, options) =>
        {
            var dbSettings = sp.GetRequiredService<IOptions<DatabaseOptions>>().Value;
            Console.WriteLine(dbSettings.SqlDataConnectionString);
            options.UseSqlServer(dbSettings.SqlDataConnectionString);
        });

        return services;
    }

    public static void ApplyMigrations(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<WeatherDbContext>>();
        var db = scope.ServiceProvider.GetRequiredService<WeatherDbContext>();

        const int maxAttempts = 5;
        var delay = TimeSpan.FromSeconds(5);

        for (var attemptNr = 1; attemptNr <= maxAttempts; attemptNr++)
            try
            {
                logger.LogInformation("Applying EF Core migrations...");
                db.Database.Migrate();
                logger.LogInformation("Migrations applied successfully.");
                break;
            }
            catch (Exception e)
            {
                logger.LogError(e, "Migration attempt {Attempt} failed.", attemptNr);

                if (attemptNr == maxAttempts)
                {
                    logger.LogError("Max retry attempts reached. Migration aborted.");
                    throw;
                }

                Thread.Sleep(delay);
            }
    }
}