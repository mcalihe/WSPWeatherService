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
public class MeasurementsServiceGetAverageAsyncTest
{
    [Fact]
    public async Task GetAverageAsync_ReturnsCorrectAverage()
    {
        // Arrange
        var (db, service) = Setup();

        var temp1 = 32;
        var temp2 = 60.0;
        db.Measurements.AddRange(new MeasurementEntity
            {
                Station = "Mythenquai",
                Timestamp = DateTimeOffset.UtcNow,
                Type = MeasurementType.AirTemperature,
                Value = temp1,
                Unit = "°C"
            },
            new MeasurementEntity
            {
                Station = "Mythenquai",
                Timestamp = DateTimeOffset.UtcNow,
                Type = MeasurementType.AirTemperature,
                Value = temp2,
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
        var result = await service.GetAverageAsync(query);

        // Assert
        Assert.NotNull(result);
        Assert.Equal((temp1 + temp2) / 2, result.Value);
    }

    [Fact]
    public async Task GetAverageAsync_ReturnsCorrectAverage_WithMultipleUnits()
    {
        // Arrange
        var (db, service) = Setup();

        var temp1 = 20;
        var temp2 = 30.0;
        db.Measurements.AddRange(new MeasurementEntity
            {
                Station = "Mythenquai",
                Timestamp = DateTimeOffset.UtcNow,
                Type = MeasurementType.AirTemperature,
                Value = temp1,
                Unit = "°C"
            }, new MeasurementEntity
            {
                Station = "Mythenquai",
                Timestamp = DateTimeOffset.UtcNow,
                Type = MeasurementType.AirTemperature,
                Value = 60.0,
                Unit = "°F"
            },
            new MeasurementEntity
            {
                Station = "Mythenquai",
                Timestamp = DateTimeOffset.UtcNow,
                Type = MeasurementType.AirTemperature,
                Value = temp2,
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
        var result = await service.GetAverageAsync(query);

        // Assert
        Assert.NotNull(result);
        Assert.Equal((temp1 + temp2) / 2, result.Value);
    }


    [Fact]
    public async Task GetAverageAsync_ReturnsNullWhenNoEntries()
    {
        // Arrange
        var (_, service) = Setup();

        var query = new MeasurementAggregationQuery(
            MeasurementType.AirTemperature,
            DateTimeOffset.UtcNow.AddDays(-1),
            DateTimeOffset.UtcNow.AddDays(1),
            null,
            "°C"
        );

        // Act
        var result = await service.GetAverageAsync(query);

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