using Hangfire;
using Tecdottir.WeatherClient;
using WSPWeatherService;
using WSPWeatherService.Extensions;
using WSPWeatherService.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddOptions<DatabaseOptions>()
    .Bind(builder.Configuration.GetSection(DatabaseOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddWeatherDbContext();
builder.Services.AddHangfireServices();
builder.Services.AddMvcCore().AddApiExplorer();
builder.Services.AddOpenApiDocument(config => { config.Title = "WSP Weather Service"; });

builder.Services.AddHttpClient();
builder.Services.AddScoped<WeatherClient>();
builder.Services.AddScoped<WeatherDataFetchJob>();
builder.Services.AddScoped<IWeatherDataFetcher, WeatherDataFetcher>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseOpenApi();
    app.UseSwaggerUi();
}

app.ApplyMigrations();
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = [new HangfireDashboardAuthorizationFilter()]
});

RecurringJob.AddOrUpdate<WeatherDataFetchJob>(
    "daily-weather-fetch",
    job => job.ExecuteAsync(),
    "30 0 * * *", // 00:30 Uhr
    new RecurringJobOptions
    {
        TimeZone = TimeZoneInfo.Local
    }
);

app.MapGet("/", () => "Api is currently running.");

app.Run();