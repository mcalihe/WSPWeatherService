using Microsoft.EntityFrameworkCore;
using WSPWeatherService.Persistence.Configurations;
using WSPWeatherService.Persistence.Models;

namespace WSPWeatherService.Persistence;

public class WeatherDbContext : DbContext
{
    public WeatherDbContext(DbContextOptions<WeatherDbContext> options)
        : base(options)
    {
    }

    public DbSet<MeasurementEntity> Measurements => Set<MeasurementEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new MeasurementEntityConfiguration());
    }
}