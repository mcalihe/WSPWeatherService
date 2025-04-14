using Microsoft.EntityFrameworkCore;

namespace WSPWeatherService.Persistence;

public class WeatherDbContext : DbContext
{
    public WeatherDbContext(DbContextOptions<WeatherDbContext> options)
        : base(options) { }
}