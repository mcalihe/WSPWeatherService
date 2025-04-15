using Microsoft.AspNetCore.Mvc;
using WSPWeatherService.Application;
using WSPWeatherService.Application.Models;
using WSPWeatherService.Persistence.Models;

namespace WSPWeatherService.Extensions;

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
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .Produces<IEnumerable<MeasurementDto>>();


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
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .Produces<string[]>();

        // This is an additional endpoint so the consumer can get all the stations
        // that are currently available
        measurementsGroup.MapGet("/stations", async (
                [FromServices] IMeasurementsService service,
                CancellationToken ct) => Results.Ok(await service.GetStations(ct)))
            .WithName("GetAllStations")
            .Produces<string[]>();


        var statsGroup = measurementsGroup.MapGroup("/stats");

        statsGroup.MapGet("/max", async (
                [FromServices] IMeasurementsService service,
                [AsParameters] MeasurementAggregationQuery query,
                CancellationToken ct) =>
            {
                var result = await service.GetMaxAsync(query, ct);
                return result is null ? Results.NotFound() : Results.Ok(result);
            }).WithName("GetMaxOfMeasurements")
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .Produces<MeasurementDto>();

        statsGroup.MapGet("/min", async (
                [FromServices] IMeasurementsService service,
                [AsParameters] MeasurementAggregationQuery query,
                CancellationToken ct) =>
            {
                var result = await service.GetMinAsync(query, ct);
                return result is null ? Results.NotFound() : Results.Ok(result);
            }).WithName("GetMinOfMeasurements")
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .Produces<MeasurementDto>();

        statsGroup.MapGet("/avg", async (
                [FromServices] IMeasurementsService service,
                [AsParameters] MeasurementAggregationQuery query,
                CancellationToken ct) =>
            {
                var result = await service.GetAverageAsync(query, ct);
                return result is null ? Results.NotFound() : Results.Ok(result);
            }).WithName("GetAvgOfMeasurements")
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .Produces<double>();

        statsGroup.MapGet("/count", async (
                [FromServices] IMeasurementsService service,
                [AsParameters] MeasurementAggregationQuery query,
                CancellationToken ct) =>
            {
                var result = await service.GetCountAsync(query, ct);
                return Results.Ok(result);
            }).WithName("GetCountOfMeasurements")
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .Produces<int>();
    }
}