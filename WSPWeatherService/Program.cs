using System.Text.Json.Serialization;
using Hangfire;
using Tecdottir.WeatherClient;
using WSPWeatherService.Api;
using WSPWeatherService.Application.Interfaces;
using WSPWeatherService.Application.Services;
using WSPWeatherService.Infrastructure;
using WSPWeatherService.Infrastructure.Jobs;
using WSPWeatherService.Options;
using WSPWeatherService.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateOnBuild = true;
    options.ValidateScopes = true;
});

builder.Services
    .AddOptions<DatabaseOptions>()
    .Bind(builder.Configuration.GetSection(DatabaseOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddWeatherDbContext();
builder.Services.AddHangfireServices();

builder.Services
    .AddControllers()
    .AddJsonOptions(opt => { opt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()); });
builder.Services.AddOpenApiDocument(config =>
{
    config.Title = "WSP Weather Service";
    config.Version = "v1";
    config.Description =
        "Provides access to validated historical weather data from the Zürich Water Police (Mythenquai & Tiefenbrunnen). " +
        "Supports queries for max, min, average and count metrics within a given time range and measurement type.\n\n" +
        "Data is fetched daily at 00:30 and available via this API.";
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ExceptionToProblemDetailsHandler>();

builder.Services.AddHttpClient();
builder.Services.AddScoped<WeatherClient>();
builder.Services.AddScoped<WeatherDataFetchJob>();
builder.Services.AddScoped<IMeasurementsService, MeasurementsService>();
builder.Services.AddScoped<IWeatherDataFetcher, WeatherDataFetcher>();

var app = builder.Build();

app.UseExceptionHandler();

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

// Let the job run once at the startup
BackgroundJob.Enqueue<WeatherDataFetchJob>(job => job.ExecuteAsync());

app.MapDefaultEndpoint(app.Environment);
app.MapMeasurementEndpoints();

if (app.Environment.IsDevelopment())
{
    app.UseOpenApi();
    app.UseSwaggerUi();
}

app.Run();