using Microsoft.EntityFrameworkCore;
using WSPWeatherService.Persistence;

namespace WSPWeatherService.Tests;

public static class TestDbContextFactory
{
    public static WeatherDbContext Create(string dbName)
    {
        var options = new DbContextOptionsBuilder<WeatherDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        return new WeatherDbContext(options);
    }
}