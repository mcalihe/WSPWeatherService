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

var app = builder.Build();

app.ApplyMigrations();
app.UseHangfireDashboardWithJobs();

app.MapGet("/", () => "Api is currently running.");

app.Run();