using System.Text.Json.Serialization;
using Hangfire;
using Tecdottir.WeatherClient;
using WSPWeatherService;
using WSPWeatherService.Application;
using WSPWeatherService.Extensions;
using WSPWeatherService.Options;

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
builder.Services.AddMvcCore().AddApiExplorer();
builder.Services.AddOpenApiDocument(config => { config.Title = "WSP Weather Service"; });
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

app.MapGet("/", () => "Api is currently running.").ExcludeFromDescription();
app.MapMeasurementEndpoints();

if (app.Environment.IsDevelopment())
{
    app.UseOpenApi();
    app.UseSwaggerUi();
}

app.Run();