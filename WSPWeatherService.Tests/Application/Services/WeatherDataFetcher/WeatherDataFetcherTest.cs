using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Tecdottir.WeatherClient;
using WSPWeatherService.Persistence.Models;
using Xunit;

namespace WSPWeatherService.Tests.Application.Services.WeatherDataFetcher;

[TestSubject(typeof(WSPWeatherService.Application.Services.WeatherDataFetcher))]
public class WeatherDataFetcherTest
{
    [Fact]
    public void MapToMeasurementEntities_MapsCorrectly()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var response = new[]
        {
            new MeasurementResponse
            {
                Station = "TestStation",
                Timestamp = now,
                Values = new Measurement
                {
                    Air_temperature = new Air_temperature
                    {
                        Status = Air_temperatureStatus.Ok,
                        Unit = "°C",
                        Value = 22.5
                    },
                    Humidity = new Humidity
                    {
                        Status = HumidityStatus.Ok,
                        Unit = "%",
                        Value = 22
                    },
                    Barometric_pressure_qfe = new Barometric_pressure_qfe
                    {
                        Status = Barometric_pressure_qfeStatus.Ok,
                        Unit = "hPa",
                        Value = 4
                    },
                    Water_temperature = new Water_temperature
                    {
                        Status = Water_temperatureStatus.Ok,
                        Unit = "°C",
                        Value = 13
                    }
                }
            }
        };

        // Act
        var result = WSPWeatherService.Application.Services.WeatherDataFetcher.MapToMeasurementEntities(response);

        // Assert
        Assert.Collection(result, m =>
            {
                Assert.Equal("TestStation", m.Station);
                Assert.Equal(now, m.Timestamp);
                Assert.Equal(MeasurementType.AirTemperature, m.Type);
                Assert.Equal(22.5, m.Value);
                Assert.Equal("°C", m.Unit);
            },
            m =>
            {
                Assert.Equal("TestStation", m.Station);
                Assert.Equal(now, m.Timestamp);
                Assert.Equal(MeasurementType.WaterTemperature, m.Type);
                Assert.Equal(13, m.Value);
                Assert.Equal("°C", m.Unit);
            },
            m =>
            {
                Assert.Equal("TestStation", m.Station);
                Assert.Equal(now, m.Timestamp);
                Assert.Equal(MeasurementType.AirPressure, m.Type);
                Assert.Equal(4, m.Value);
                Assert.Equal("hPa", m.Unit);
            },
            m =>
            {
                Assert.Equal("TestStation", m.Station);
                Assert.Equal(now, m.Timestamp);
                Assert.Equal(MeasurementType.Humidity, m.Type);
                Assert.Equal(22, m.Value);
                Assert.Equal("%", m.Unit);
            });
    }

    [Fact]
    public void MapToMeasurementEntities_DoesNotMapNullEntries()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var response = new[]
        {
            new MeasurementResponse
            {
                Station = "TestStation",
                Timestamp = now,
                Values = new Measurement
                {
                    Air_temperature = new Air_temperature
                    {
                        Status = Air_temperatureStatus.Ok,
                        Unit = "°C",
                        Value = null
                    },
                    Humidity = new Humidity
                    {
                        Status = HumidityStatus.Ok,
                        Unit = "%",
                        Value = null
                    }
                }
            }
        };

        // Act
        var result = WSPWeatherService.Application.Services.WeatherDataFetcher.MapToMeasurementEntities(response);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void MapToMeasurementEntities_DoesNotMapFaultyEntries()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var response = new[]
        {
            new MeasurementResponse
            {
                Station = "TestStation",
                Timestamp = now,
                Values = new Measurement
                {
                    Air_temperature = new Air_temperature
                    {
                        Status = Air_temperatureStatus.Broken,
                        Unit = "°C",
                        Value = 22.5
                    },
                    Humidity = new Humidity
                    {
                        Status = HumidityStatus.Broken,
                        Unit = "%",
                        Value = 22
                    }
                }
            }
        };

        // Act
        var result = WSPWeatherService.Application.Services.WeatherDataFetcher.MapToMeasurementEntities(response);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task FilterAlreadyExistingEntriesAsync_FiltersOutAlreadyExistingEntries()
    {
        // Arrange
        var db = TestDbContextFactory.Create(Guid.NewGuid().ToString());
        var mockClient = new Mock<WeatherClient>(Mock.Of<HttpClient>());
        var fetcher = new WSPWeatherService.Application.Services.WeatherDataFetcher(
            Mock.Of<ILogger<WSPWeatherService.Application.Services.WeatherDataFetcher>>(), db,
            mockClient.Object);

        var now = DateTimeOffset.UtcNow;
        var existing = new MeasurementEntity
        {
            Station = "Mythenquai",
            Timestamp = now,
            Type = MeasurementType.AirTemperature,
            Value = 20.0,
            Unit = "°C"
        };

        await db.Measurements.AddAsync(existing);
        await db.SaveChangesAsync();

        var input = new[]
        {
            existing,
            new MeasurementEntity
            {
                Station = "Tiefenbrunnen",
                Timestamp = now,
                Type = MeasurementType.WaterTemperature,
                Value = 18,
                Unit = "°C"
            }
        };

        // Act
        var filtered =
            await fetcher.FilterAlreadyExistingEntriesAsync(input, now.AddDays(-1).DateTime, now.AddDays(1).DateTime);

        // Assert
        Assert.Single(filtered);
        Assert.Equal("Tiefenbrunnen", filtered[0].Station);
    }

    [Fact]
    public async Task FetchAsync_ReturnsAllMeasurements()
    {
        // Arrange
        var mockLogger = Mock.Of<ILogger<WSPWeatherService.Application.Services.WeatherDataFetcher>>();
        var db = TestDbContextFactory.Create(Guid.NewGuid().ToString());

        var now = DateTimeOffset.UtcNow;

        var mockClient = new Mock<WeatherClient>(Mock.Of<HttpClient>());
        mockClient.Setup(c => c.MeasurementsAsync(
                Station2.Mythenquai,
                It.IsAny<DateTimeOffset>(),
                It.IsAny<DateTimeOffset>(),
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<int?>(),
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(new MeasurementApiResponse
            {
                Ok = true,
                Result =
                [
                    new MeasurementResponse
                    {
                        Station = "Mythenquai",
                        Timestamp = now,
                        Values = new Measurement
                        {
                            Air_temperature = new Air_temperature
                            {
                                Status = Air_temperatureStatus.Ok,
                                Unit = "°C",
                                Value = 22.0
                            }
                        }
                    }
                ]
            });
        mockClient.Setup(c => c.MeasurementsAsync(
                Station2.Tiefenbrunnen,
                It.IsAny<DateTimeOffset>(),
                It.IsAny<DateTimeOffset>(),
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<int?>(),
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(new MeasurementApiResponse
            {
                Ok = true,
                Result =
                [
                    new MeasurementResponse
                    {
                        Station = "Tiefenbrunnen",
                        Timestamp = now,
                        Values = new Measurement
                        {
                            Air_temperature = new Air_temperature
                            {
                                Status = Air_temperatureStatus.Ok,
                                Unit = "°C",
                                Value = 35
                            }
                        }
                    }
                ]
            });

        var fetcher = new WSPWeatherService.Application.Services.WeatherDataFetcher(mockLogger, db, mockClient.Object);
        var start = DateTime.UtcNow.AddDays(-1);
        var end = DateTime.UtcNow;

        // Act
        var result = await fetcher.FetchAsync(start, end);

        // Assert
        Assert.Collection(result, r =>
            {
                Assert.Equal("Mythenquai", r.Station);
                Assert.Equal(now, r.Timestamp);
                Assert.Equal("°C", r.Values.Air_temperature.Unit);
                Assert.Equal(Air_temperatureStatus.Ok, r.Values.Air_temperature.Status);
                Assert.Equal(22.0, r.Values.Air_temperature.Value);
            },
            r =>
            {
                Assert.Equal("Tiefenbrunnen", r.Station);
                Assert.Equal(now, r.Timestamp);
                Assert.Equal("°C", r.Values.Air_temperature.Unit);
                Assert.Equal(Air_temperatureStatus.Ok, r.Values.Air_temperature.Status);
                Assert.Equal(35, r.Values.Air_temperature.Value);
            });
    }

    [Fact]
    public async Task FetchAsync_ReturnsMeasurements_EvenIfOneIsNotOK()
    {
        // Arrange
        var mockLogger = Mock.Of<ILogger<WSPWeatherService.Application.Services.WeatherDataFetcher>>();
        var db = TestDbContextFactory.Create(Guid.NewGuid().ToString());

        var now = DateTimeOffset.UtcNow;
        var expectedStation = "Mythenquai";

        var mockClient = new Mock<WeatherClient>(Mock.Of<HttpClient>());
        mockClient.Setup(c => c.MeasurementsAsync(
                Station2.Mythenquai,
                It.IsAny<DateTimeOffset>(),
                It.IsAny<DateTimeOffset>(),
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<int?>(),
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(new MeasurementApiResponse
            {
                Ok = true,
                Result =
                [
                    new MeasurementResponse
                    {
                        Station = expectedStation,
                        Timestamp = now,
                        Values = new Measurement
                        {
                            Air_temperature = new Air_temperature
                            {
                                Status = Air_temperatureStatus.Ok,
                                Unit = "°C",
                                Value = 22.0
                            }
                        }
                    }
                ]
            });
        mockClient.Setup(c => c.MeasurementsAsync(
                Station2.Tiefenbrunnen,
                It.IsAny<DateTimeOffset>(),
                It.IsAny<DateTimeOffset>(),
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<int?>(),
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(new MeasurementApiResponse
            {
                Ok = false,
                Result = new List<MeasurementResponse>()
            });

        var fetcher = new WSPWeatherService.Application.Services.WeatherDataFetcher(mockLogger, db, mockClient.Object);
        var start = DateTime.UtcNow.AddDays(-1);
        var end = DateTime.UtcNow;

        // Act
        var result = await fetcher.FetchAsync(start, end);

        // Assert
        Assert.Single(result);
        Assert.Equal(expectedStation, result[0].Station);
        Assert.Equal(now, result[0].Timestamp);
        Assert.Equal("°C", result[0].Values.Air_temperature.Unit);
        Assert.Equal(Air_temperatureStatus.Ok, result[0].Values.Air_temperature.Status);
        Assert.Equal(22.0, result[0].Values.Air_temperature.Value);
    }

    [Fact]
    public async Task FetchAsync_ReturnsMeasurements_EvenIfThereIsAnException()
    {
        // Arrange
        var mockLogger = Mock.Of<ILogger<WSPWeatherService.Application.Services.WeatherDataFetcher>>();
        var db = TestDbContextFactory.Create(Guid.NewGuid().ToString());

        var now = DateTimeOffset.UtcNow;
        var expectedStation = "Mythenquai";

        var mockClient = new Mock<WeatherClient>(Mock.Of<HttpClient>());
        mockClient.Setup(c => c.MeasurementsAsync(
                Station2.Mythenquai,
                It.IsAny<DateTimeOffset>(),
                It.IsAny<DateTimeOffset>(),
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<int?>(),
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(new MeasurementApiResponse
            {
                Ok = true,
                Result =
                [
                    new MeasurementResponse
                    {
                        Station = expectedStation,
                        Timestamp = now,
                        Values = new Measurement
                        {
                            Air_temperature = new Air_temperature
                            {
                                Status = Air_temperatureStatus.Ok,
                                Unit = "°C",
                                Value = 22.0
                            }
                        }
                    }
                ]
            });
        mockClient.Setup(c => c.MeasurementsAsync(
                Station2.Tiefenbrunnen,
                It.IsAny<DateTimeOffset>(),
                It.IsAny<DateTimeOffset>(),
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<int?>(),
                It.IsAny<CancellationToken>()
            ))
            .Throws<TimeoutException>();

        var fetcher = new WSPWeatherService.Application.Services.WeatherDataFetcher(mockLogger, db, mockClient.Object);
        var start = DateTime.UtcNow.AddDays(-1);
        var end = DateTime.UtcNow;

        // Act
        var result = await fetcher.FetchAsync(start, end);

        // Assert
        Assert.Single(result);
        Assert.Equal(expectedStation, result[0].Station);
        Assert.Equal("°C", result[0].Values.Air_temperature.Unit);
        Assert.Equal(Air_temperatureStatus.Ok, result[0].Values.Air_temperature.Status);
        Assert.Equal(22.0, result[0].Values.Air_temperature.Value);
    }

    [Fact]
    public async Task FetchAndStoreAsync_InsertsOnlyNewMeasurements()
    {
        // Arrange
        var db = TestDbContextFactory.Create(Guid.NewGuid().ToString());

        var now = DateTimeOffset.UtcNow;

        var existingMeasurement = new MeasurementEntity
        {
            Station = "Mythenquai",
            Timestamp = now,
            Type = MeasurementType.AirTemperature,
            Value = 21.0,
            Unit = "°C"
        };

        await db.Measurements.AddAsync(existingMeasurement);
        await db.SaveChangesAsync();

        var responseMeasurement = new MeasurementResponse
        {
            Station = "Mythenquai",
            Timestamp = now,
            Values = new Measurement
            {
                Air_temperature = new Air_temperature
                {
                    Status = Air_temperatureStatus.Ok,
                    Value = 21.0,
                    Unit = "°C"
                },
                Humidity = new Humidity
                {
                    Status = HumidityStatus.Ok,
                    Value = 42,
                    Unit = "%"
                }
            }
        };

        var mockClient = new Mock<WeatherClient>(Mock.Of<HttpClient>());

        mockClient.Setup(c => c.MeasurementsAsync(
                Station2.Mythenquai,
                It.IsAny<DateTimeOffset>(),
                It.IsAny<DateTimeOffset>(),
                It.IsAny<string>(),
                It.IsAny<int>(),
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MeasurementApiResponse
            {
                Ok = true,
                Result = new List<MeasurementResponse> { responseMeasurement }
            });

        var logger = Mock.Of<ILogger<WSPWeatherService.Application.Services.WeatherDataFetcher>>();

        var fetcher = new WSPWeatherService.Application.Services.WeatherDataFetcher(logger, db, mockClient.Object);

        // Act
        await fetcher.FetchAndStoreAsync(now.AddDays(-1), now.AddDays(1));

        // Assert
        var measurements = await db.Measurements.ToListAsync();

        Assert.Collection(measurements, m =>
        {
            Assert.Equal("Mythenquai", m.Station);
            Assert.Equal(now, m.Timestamp);
            Assert.Equal(MeasurementType.AirTemperature, m.Type);
            Assert.Equal(21.0, m.Value);
            Assert.Equal("°C", m.Unit);
        }, m =>
        {
            Assert.Equal("Mythenquai", m.Station);
            Assert.Equal(now, m.Timestamp);
            Assert.Equal(MeasurementType.Humidity, m.Type);
            Assert.Equal(42, m.Value);
            Assert.Equal("%", m.Unit);
        });
    }

    [Fact]
    public async Task FetchAndStoreAsync_ThrowsIfStartIsAfterEndDate()
    {
        // Arrange
        var db = TestDbContextFactory.Create(Guid.NewGuid().ToString());
        var logger = Mock.Of<ILogger<WSPWeatherService.Application.Services.WeatherDataFetcher>>();
        var mockClient = new Mock<WeatherClient>(Mock.Of<HttpClient>());

        var now = DateTimeOffset.UtcNow;
        var fetcher = new WSPWeatherService.Application.Services.WeatherDataFetcher(logger, db, mockClient.Object);
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => fetcher.FetchAndStoreAsync(now.AddDays(1), now.AddDays(-1)));
    }
}