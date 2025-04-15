using Hangfire;
using Microsoft.Extensions.Options;
using WSPWeatherService.Options;

namespace WSPWeatherService.Infrastructure;

public static class InfrastructureExtensions
{
    public static IServiceCollection AddHangfireServices(this IServiceCollection services)
    {
        services.AddHangfire((sp, config) =>
        {
            var dbOptions = sp.GetRequiredService<IOptions<DatabaseOptions>>().Value;
            config.UseSqlServerStorage(dbOptions.HangfireConnectionString);
        });

        services.AddHangfireServer();

        return services;
    }
}