using Hangfire.Dashboard;

namespace WSPWeatherService.Infrastructure;

public class HangfireDashboardAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();
        var env = httpContext.RequestServices.GetRequiredService<IWebHostEnvironment>();

        if (env.IsDevelopment())
        {
            return true;
        }

        // No auth implemented for this example. Else there should be a ways to authorize to non Development Hangfire Dashboard
        return false;
    }
}