using Hangfire;
using Microsoft.Extensions.Options;
using WSPWeatherService.Options;

namespace WSPWeatherService.Extensions;

public static class HangfireExtensions
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

    public static IApplicationBuilder UseHangfireDashboardWithJobs(this IApplicationBuilder app)
    {
        return app;
    }
}