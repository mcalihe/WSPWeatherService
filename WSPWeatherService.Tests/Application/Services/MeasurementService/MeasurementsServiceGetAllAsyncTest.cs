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
public class MeasurementsServiceGetAllAsyncTest
{
    [Fact]
    public async Task GetAllAsync_ReturnsEmptyCollection_WhenEmpty()
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
        var result = await service.GetAllAsync(query);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsCorrectCollection()
    {
        // Arrange
        var (db, service) = Setup();

        var measurement1 = new MeasurementEntity
        {
            Station = "Mythenquai",
            Timestamp = DateTimeOffset.UtcNow,
            Type = MeasurementType.AirTemperature,
            Value = 3.0,
            Unit = "°F"
        };
        var measurement2 = new MeasurementEntity
        {
            Station = "Mythenquai",
            Timestamp = DateTimeOffset.UtcNow,
            Type = MeasurementType.AirTemperature,
            Value = 18,
            Unit = "°C"
        };
        var measurement3 = new MeasurementEntity
        {
            Station = "Mythenquai",
            Timestamp = DateTimeOffset.UtcNow,
            Type = MeasurementType.AirTemperature,
            Value = 30,
            Unit = "°C"
        };
        db.Measurements.AddRange(measurement1, measurement2, measurement3);
        await db.SaveChangesAsync();

        var query = new MeasurementQuery(
            MeasurementType.AirTemperature,
            DateTimeOffset.UtcNow.AddDays(-1),
            DateTimeOffset.UtcNow.AddDays(1),
            null
        );

        // Act
        var result = await service.GetAllAsync(query);

        // Assert
        Assert.Collection(result, m =>
        {
            Assert.Equal(measurement1.Station, m.Station);
            Assert.Equal(measurement1.Type, m.Type);
            Assert.Equal(measurement1.Unit, m.Unit);
            Assert.Equal(measurement1.Value, m.Value, 10);
            Assert.Equal(measurement1.Timestamp, m.Timestamp);
        }, m =>
        {
            Assert.Equal(measurement2.Station, m.Station);
            Assert.Equal(measurement2.Type, m.Type);
            Assert.Equal(measurement2.Unit, m.Unit);
            Assert.Equal(measurement2.Value, m.Value, 10);
            Assert.Equal(measurement2.Timestamp, m.Timestamp);
        }, m =>
        {
            Assert.Equal(measurement3.Station, m.Station);
            Assert.Equal(measurement3.Type, m.Type);
            Assert.Equal(measurement3.Unit, m.Unit);
            Assert.Equal(measurement3.Value, m.Value, 10);
            Assert.Equal(measurement3.Timestamp, m.Timestamp);
        });
    }

    private (WeatherDbContext db, MeasurementsService service) Setup()
    {
        var db = TestDbContextFactory.Create(Guid.NewGuid().ToString());
        var service = new MeasurementsService(db);

        return (db, service);
    }
}