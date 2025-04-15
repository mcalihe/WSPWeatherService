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
public class MeasurementsServiceGetCountAsyncTest
{
    [Fact]
    public async Task GetCountAsync_ReturnsCorrectCount_WhenEmpty()
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
        var result = await service.GetCountAsync(query);

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public async Task GetCountAsync_ReturnsCorrectCount_WithMultipleUnits()
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
        var result = await service.GetCountAsync(query);

        // Assert
        Assert.Equal(2, result);
    }

    [Fact]
    public async Task GetCountAsync_ReturnsEmptyCollection_WhenNoEntriesForThisType()
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
        var result = await service.GetUnits(MeasurementType.Humidity);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetCountAsync_ReturnsCorrectCollectionForType()
    {
        // Arrange
        var (db, service) = Setup();

        var unit1 = "U1";
        var unit2 = "U2";
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
                Type = MeasurementType.WaterTemperature,
                Value = 18,
                Unit = unit1
            },
            new MeasurementEntity
            {
                Station = "Tiefenbrunnen",
                Timestamp = DateTimeOffset.UtcNow,
                Type = MeasurementType.WaterTemperature,
                Value = 38,
                Unit = unit1
            },
            new MeasurementEntity
            {
                Station = "Mythenquai",
                Timestamp = DateTimeOffset.UtcNow,
                Type = MeasurementType.WaterTemperature,
                Value = 18,
                Unit = unit2
            }, new MeasurementEntity
            {
                Station = "Mythenquai",
                Timestamp = DateTimeOffset.UtcNow,
                Type = MeasurementType.AirTemperature,
                Value = 30,
                Unit = "°C"
            });
        await db.SaveChangesAsync();

        // Act
        var result = await service.GetUnits(MeasurementType.WaterTemperature);

        // Assert
        Assert.Collection(result, u => Assert.Equal(unit1, u), u => Assert.Equal(unit2, u));
    }


    private (WeatherDbContext db, MeasurementsService service) Setup()
    {
        var db = TestDbContextFactory.Create(Guid.NewGuid().ToString());
        var service = new MeasurementsService(db);

        return (db, service);
    }
}