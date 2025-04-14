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

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseOpenApi();
    app.UseSwaggerUi();
}

app.ApplyMigrations();
app.UseHangfireDashboardWithJobs();

app.MapGet("/", () => "Api is currently running.");

app.Run();