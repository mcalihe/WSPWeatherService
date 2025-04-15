using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using WSPWeatherService.Api.Models;
using WSPWeatherService.Application.Services;
using WSPWeatherService.Persistence;
using WSPWeatherService.Persistence.Models;
using Xunit;

namespace WSPWeatherService.Tests.Application.Services.MeasurementService;

[TestSubject(typeof(MeasurementsService))]
public class MeasurementsServiceGetMinAsyncTest
{
    [Fact]
    public async Task GetMinAsync_ReturnsCorrectMax()
    {
        // Arrange
        var (db, service) = Setup();

        db.Measurements.AddRange(new MeasurementEntity
        {
            Station = "Mythenquai",
            Timestamp = DateTimeOffset.UtcNow,
            Type = MeasurementType.AirTemperature,
            Value = 21.5,
            Unit = "°C"
        }, new MeasurementEntity
        {
            Station = "Mythenquai",
            Timestamp = DateTimeOffset.UtcNow,
            Type = MeasurementType.AirTemperature,
            Value = 25.0,
            Unit = "°C"
        });
        await db.SaveChangesAsync();

        var query = new MeasurementAggregationQuery(
            MeasurementType.AirTemperature,
            DateTimeOffset.UtcNow.AddDays(-1),
            DateTimeOffset.UtcNow.AddDays(1),
            null,
            "°C"
        );

        // Act
        var result = await service.GetMinAsync(query);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(21.5, result.Value);
        Assert.Equal("Mythenquai", result.Station);
        Assert.Equal("°C", result.Unit);
    }

    [Fact]
    public async Task GetMinAsync_ReturnsCorrectMax_WithMultipleUnits()
    {
        // Arrange
        var (db, service) = Setup();

        db.Measurements.AddRange(
            new MeasurementEntity
            {
                Station = "Mythenquai",
                Timestamp = DateTimeOffset.UtcNow,
                Type = MeasurementType.AirTemperature,
                Value = 3.0,
                Unit = "°F"
            }, new MeasurementEntity
            {
                Station = "Mythenquai",
                Timestamp = DateTimeOffset.UtcNow,
                Type = MeasurementType.AirTemperature,
                Value = 18,
                Unit = "°C"
            }, new MeasurementEntity
            {
                Station = "Mythenquai",
                Timestamp = DateTimeOffset.UtcNow,
                Type = MeasurementType.AirTemperature,
                Value = 30,
                Unit = "°C"
            });
        await db.SaveChangesAsync();

        var query = new MeasurementAggregationQuery(
            MeasurementType.AirTemperature,
            DateTimeOffset.UtcNow.AddDays(-1),
            DateTimeOffset.UtcNow.AddDays(1),
            null,
            "°C"
        );

        // Act
        var result = await service.GetMinAsync(query);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(18, result.Value);
        Assert.Equal("Mythenquai", result.Station);
        Assert.Equal("°C", result.Unit);
    }

    [Fact]
    public async Task GetMinAsync_ReturnsNullWhenNoEntries()
    {
        // Arrange
        var (_, service) = Setup();

        var query = new MeasurementAggregationQuery(
            MeasurementType.AirTemperature,
            DateTimeOffset.UtcNow.AddDays(-1),
            DateTimeOffset.UtcNow.AddDays(1),
            "Mythenquai",
            "°C"
        );

        // Act
        var result = await service.GetMinAsync(query);

        // Assert
        Assert.Null(result);
    }

    private (WeatherDbContext db, MeasurementsService service) Setup()
    {
        var db = TestDbContextFactory.Create(Guid.NewGuid().ToString());
        var service = new MeasurementsService(db);

        return (db, service);
    }
}