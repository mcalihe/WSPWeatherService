using Hangfire;
using Hangfire.Dashboard;
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
        app.UseHangfireDashboard("/hangfire", new DashboardOptions
        {
            Authorization = [new MyAuthorizationFilter()]
        });

        RecurringJob.AddOrUpdate("my-job", () => Console.WriteLine("Recurring job!"), Cron.Minutely);

        return app;
    }

    private class MyAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            var httpContext = context.GetHttpContext();
            var env = httpContext.RequestServices.GetRequiredService<IWebHostEnvironment>();

            if (env.EnvironmentName == Environments.Development) return true;

            // No auth implemented for this example. Else there should be a ways to authorize to non Development Hangfire Dashboard
            return false;
        }
    }
}