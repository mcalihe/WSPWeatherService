using Microsoft.AspNetCore.Mvc;
using WSPWeatherService.Api.Models;
using WSPWeatherService.Application.Interfaces;
using WSPWeatherService.Persistence.Models;

namespace WSPWeatherService.Api;

public static class MeasurementsEndpointsExtensions
{
    public static void MapMeasurementEndpoints(this IEndpointRouteBuilder routeBuilder)
    {
        var measurementsGroup = routeBuilder
            .MapGroup("/measurements")
            .WithTags("Measurements");


        // WARNING: This endpoint can severely impact API performance.
        // Consider introducing stricter limitations before using it in production.
        //
        // Suggestions:
        // - Enforce a maximum allowed date range (e.g. 6 months)
        // - Enforce a maximum number of rows per request (e.g. 5000)
        // - Support pagination using cursor or skip/take
        measurementsGroup.MapGet("", async (
                [FromServices] IMeasurementsService service,
                [AsParameters] MeasurementQuery query,
                CancellationToken ct) =>
            {
                var result = await service.GetAllAsync(query, ct);
                return Results.Ok(result);
            }).WithName("GetAllMeasurements")
            .WithSummary("Returns all measurements within the given range.")
            .WithDescription(
                "Retrieves a list of all stored measurements filtered by required `type` & `time` range and optional `station`.")
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .Produces<IEnumerable<MeasurementDto>>()
            .WithOpenApi();


        // This is an additional endpoint so the consumer can get all the units
        // that are currently available for a specific type
        //
        // I would also be a possibility to make the units strongly typed (with an enum per type for example)
        // But this would come with some benefits and some drawbacks as always.
        measurementsGroup.MapGet("/units", async (
                [FromQuery] MeasurementType type,
                [FromServices] IMeasurementsService service,
                CancellationToken ct) => Results.Ok(await service.GetUnits(type, ct)))
            .WithName("GetUnitsForMeasurementType")
            .WithSummary("Get available units for a given measurement type.")
            .WithDescription(
                "Returns a list of all units that exist for the specified `MeasurementType`.")
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .Produces<string[]>()
            .WithOpenApi();

        // This is an additional endpoint so the consumer can get all the stations
        // that are currently available
        measurementsGroup.MapGet("/stations", async (
                [FromServices] IMeasurementsService service,
                CancellationToken ct) => Results.Ok(await service.GetStations(ct)))
            .WithName("GetAllStations")
            .WithSummary("Returns all available stations.")
            .WithDescription(
                "Provides a list of all station identifiers used in the system (e.g., `Mythenquai`, `Tiefenbrunnen`).")
            .Produces<string[]>()
            .WithOpenApi();


        var statsGroup = measurementsGroup.MapGroup("/stats");

        statsGroup.MapGet("/max", async (
                [FromServices] IMeasurementsService service,
                [AsParameters] MeasurementAggregationQuery query,
                CancellationToken ct) =>
            {
                var result = await service.GetMaxAsync(query, ct);
                return result is null ? Results.NotFound() : Results.Ok(result);
            }).WithName("GetMaxOfMeasurements")
            .WithSummary("Returns the maximum value for a measurement type.")
            .WithDescription(
                """
                Returns the highest recorded value for a given measurement type within a specified time range.
                Optional station filtering is supported. Returns status `404` if no data was found.
                """)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .Produces<MeasurementDto>()
            .WithOpenApi();

        statsGroup.MapGet("/min", async (
                [FromServices] IMeasurementsService service,
                [AsParameters] MeasurementAggregationQuery query,
                CancellationToken ct) =>
            {
                var result = await service.GetMinAsync(query, ct);
                return result is null ? Results.NotFound() : Results.Ok(result);
            }).WithName("GetMinOfMeasurements")
            .WithSummary("Returns the minimum value for a measurement type.")
            .WithDescription(
                """
                Returns the lowest recorded value for a given measurement type within a specified time range.
                Optional station filtering is supported. Returns status `404` if no data was found.
                """)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .Produces<MeasurementDto>()
            .WithOpenApi();

        statsGroup.MapGet("/avg", async (
                [FromServices] IMeasurementsService service,
                [AsParameters] MeasurementAggregationQuery query,
                CancellationToken ct) =>
            {
                var result = await service.GetAverageAsync(query, ct);
                return result is null ? Results.NotFound() : Results.Ok(result);
            }).WithName("GetAvgOfMeasurements")
            .WithSummary("Returns the average value for a measurement type.")
            .WithDescription(
                """
                Calculates the average of all values for a given measurement type and optional station
                within a specified time range. Returns status `404` if no data was found.
                """)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .Produces<double>()
            .WithOpenApi();

        statsGroup.MapGet("/count", async (
                [FromServices] IMeasurementsService service,
                [AsParameters] MeasurementAggregationQuery query,
                CancellationToken ct) =>
            {
                var result = await service.GetCountAsync(query, ct);
                return Results.Ok(result);
            }).WithName("GetCountOfMeasurements")
            .WithSummary("Returns the number of stored measurements.")
            .WithDescription(
                """
                Returns the total count of stored measurements matching the specified time range,
                measurement type, and optional station filter.
                """)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .Produces<int>()
            .WithOpenApi();
    }
}