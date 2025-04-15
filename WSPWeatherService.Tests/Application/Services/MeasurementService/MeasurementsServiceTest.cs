using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using WSPWeatherService.Api.Models;
using WSPWeatherService.Application.Services;
using WSPWeatherService.Persistence;
using WSPWeatherService.Persistence.Models;
using Xunit;

namespace WSPWeatherService.Tests.Application.Services.MeasurementService;

[TestSubject(typeof(MeasurementsService))]
public class MeasurementsServiceTest
{
    [Fact]
    public async Task GetStationsAsync_ReturnsEmptyCollection_WhenEmpty()
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
        var result = await service.GetStations();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetStationsAsync_ReturnsCorrectCollection()
    {
        // Arrange
        var (db, service) = Setup();

        var station1 = "Frauenfeld";
        var station2 = "Kriens";
        var station3 = "Teststation";
        var now = DateTimeOffset.UtcNow;
        db.Measurements.AddRange(
            new MeasurementEntity
            {
                Station = station1,
                Timestamp = now,
                Type = MeasurementType.WaterTemperature,
                Value = 3.0,
                Unit = "°F"
            },
            new MeasurementEntity
            {
                Station = station1,
                Timestamp = now,
                Type = MeasurementType.AirTemperature,
                Value = 3.0,
                Unit = "°F"
            }, new MeasurementEntity
            {
                Station = station2,
                Timestamp = now,
                Type = MeasurementType.AirTemperature,
                Value = 18,
                Unit = "°C"
            }, new MeasurementEntity
            {
                Station = station3,
                Timestamp = now,
                Type = MeasurementType.AirTemperature,
                Value = 30,
                Unit = "°C"
            });
        await db.SaveChangesAsync();

        // Act
        var result = await service.GetStations();

        // Assert
        Assert.Collection(result,
            s => Assert.Equal(station1, s),
            s => Assert.Equal(station2, s),
            s => Assert.Equal(station3, s));
    }

    [Fact]
    public async Task GetUnitsAsync_ReturnsCorrectCollectionForType()
    {
        // Arrange
        var (db, service) = Setup();

        var unit1 = "U1";
        var unit2 = "U2";
        var unit3 = "U3";
        db.Measurements.AddRange(
            new MeasurementEntity
            {
                Station = "Teststation",
                Timestamp = DateTimeOffset.UtcNow,
                Type = MeasurementType.WaterTemperature,
                Value = 3.0,
                Unit = unit1
            },
            new MeasurementEntity
            {
                Station = "Frauenfeld",
                Timestamp = DateTimeOffset.UtcNow,
                Type = MeasurementType.AirTemperature,
                Value = 3.0,
                Unit = unit2
            }, new MeasurementEntity
            {
                Station = "Kriens",
                Timestamp = DateTimeOffset.UtcNow,
                Type = MeasurementType.WaterTemperature,
                Value = 18,
                Unit = unit3
            });
        await db.SaveChangesAsync();

        // Act
        var result = await service.GetUnits(MeasurementType.WaterTemperature);

        // Assert
        Assert.Collection(result,
            u => Assert.Equal(unit1, u),
            u => Assert.Equal(unit3, u));
    }


    [Fact]
    public async Task ApplyQuery_FiltersTypeCorrectly()
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
            Type = MeasurementType.WaterTemperature,
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
            MeasurementType.WaterTemperature,
            DateTimeOffset.UtcNow.AddDays(-1),
            DateTimeOffset.UtcNow.AddDays(1),
            null
        );

        // Act
        var result = await service.ApplyQuery(query).ToArrayAsync();

        // Assert
        Assert.Collection(result, m =>
        {
            Assert.Equal(measurement2.Station, m.Station);
            Assert.Equal(measurement2.Type, m.Type);
            Assert.Equal(measurement2.Unit, m.Unit);
            Assert.Equal(measurement2.Value, m.Value, 10);
            Assert.Equal(measurement2.Timestamp, m.Timestamp);
        });
    }

    [Fact]
    public async Task ApplyQuery_FiltersStationCorrectly()
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
            Station = "Tiefenbrunnen",
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
            "Tiefenbrunnen"
        );

        // Act
        var result = await service.ApplyQuery(query).ToArrayAsync();

        // Assert
        Assert.Collection(result, m =>
        {
            Assert.Equal(measurement2.Station, m.Station);
            Assert.Equal(measurement2.Type, m.Type);
            Assert.Equal(measurement2.Unit, m.Unit);
            Assert.Equal(measurement2.Value, m.Value, 10);
            Assert.Equal(measurement2.Timestamp, m.Timestamp);
        });
    }

    [Fact]
    public async Task ApplyQuery_FiltersDateCorrectly()
    {
        // Arrange
        var (db, service) = Setup();
        var now = DateTimeOffset.UtcNow;
        var startOfRange = now.AddDays(-2);
        var endOfRange = now.AddDays(2);

        var measurement1 = new MeasurementEntity
        {
            Station = "Mythenquai",
            Timestamp = startOfRange.AddTicks(-1),
            Type = MeasurementType.AirTemperature,
            Value = 1,
            Unit = "°F"
        };
        var measurement2 = new MeasurementEntity
        {
            Station = "Tiefenbrunnen",
            Timestamp = startOfRange,
            Type = MeasurementType.AirTemperature,
            Value = 2,
            Unit = "°C"
        };
        var measurement3 = new MeasurementEntity
        {
            Station = "Tiefenbrunnen",
            Timestamp = startOfRange.AddDays(1),
            Type = MeasurementType.AirTemperature,
            Value = 3,
            Unit = "°C"
        };
        var measurement4 = new MeasurementEntity
        {
            Station = "Mythenquai",
            Timestamp = startOfRange.AddDays(2),
            Type = MeasurementType.AirTemperature,
            Value = 4,
            Unit = "°C"
        };
        var measurement5 = new MeasurementEntity
        {
            Station = "Mythenquai",
            Timestamp = endOfRange,
            Type = MeasurementType.AirTemperature,
            Value = 5,
            Unit = "°C"
        };
        var measurement6 = new MeasurementEntity
        {
            Station = "Mythenquai",
            Timestamp = endOfRange.AddTicks(1),
            Type = MeasurementType.AirTemperature,
            Value = 6,
            Unit = "°C"
        };
        db.Measurements.AddRange(measurement1, measurement2, measurement3, measurement4, measurement5, measurement6);
        await db.SaveChangesAsync();

        var query = new MeasurementQuery(
            MeasurementType.AirTemperature,
            startOfRange,
            endOfRange,
            null
        );

        // Act
        var result = await service.ApplyQuery(query).ToArrayAsync();

        // Assert
        Assert.Collection(result, m =>
            {
                Assert.Equal(measurement2.Station, m.Station);
                Assert.Equal(measurement2.Type, m.Type);
                Assert.Equal(measurement2.Unit, m.Unit);
                Assert.Equal(measurement2.Value, m.Value, 10);
                Assert.Equal(measurement2.Timestamp, m.Timestamp);
            },
            m =>
            {
                Assert.Equal(measurement3.Station, m.Station);
                Assert.Equal(measurement3.Type, m.Type);
                Assert.Equal(measurement3.Unit, m.Unit);
                Assert.Equal(measurement3.Value, m.Value, 10);
                Assert.Equal(measurement3.Timestamp, m.Timestamp);
            },
            m =>
            {
                Assert.Equal(measurement4.Station, m.Station);
                Assert.Equal(measurement4.Type, m.Type);
                Assert.Equal(measurement4.Unit, m.Unit);
                Assert.Equal(measurement4.Value, m.Value, 10);
                Assert.Equal(measurement4.Timestamp, m.Timestamp);
            },
            m =>
            {
                Assert.Equal(measurement5.Station, m.Station);
                Assert.Equal(measurement5.Type, m.Type);
                Assert.Equal(measurement5.Unit, m.Unit);
                Assert.Equal(measurement5.Value, m.Value, 10);
                Assert.Equal(measurement5.Timestamp, m.Timestamp);
            });
    }

    [Fact]
    public async Task ApplyQuery_FiltersUnitCorrectly()
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

        var query = new MeasurementAggregationQuery(
            MeasurementType.AirTemperature,
            DateTimeOffset.UtcNow.AddDays(-1),
            DateTimeOffset.UtcNow.AddDays(1),
            null,
            "°F"
        );

        // Act
        var result = await service.ApplyQuery(query).ToArrayAsync();

        // Assert
        Assert.Collection(result, m =>
        {
            Assert.Equal(measurement1.Station, m.Station);
            Assert.Equal(measurement1.Type, m.Type);
            Assert.Equal(measurement1.Unit, m.Unit);
            Assert.Equal(measurement1.Value, m.Value, 10);
            Assert.Equal(measurement1.Timestamp, m.Timestamp);
        });
    }


    private (WeatherDbContext db, MeasurementsService service) Setup()
    {
        var db = TestDbContextFactory.Create(Guid.NewGuid().ToString());
        var service = new MeasurementsService(db);

        return (db, service);
    }
}