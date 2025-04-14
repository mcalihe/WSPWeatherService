using Microsoft.AspNetCore.Mvc;
using WSPWeatherService.Application;
using WSPWeatherService.Application.Models;

namespace WSPWeatherService.Extensions;

public static class MeasurementsEndpointsExtensions
{
    public static void MapMeasurementEndpoints(this IEndpointRouteBuilder routeBuilder)
    {
        var measurementsGroup = routeBuilder
            .MapGroup("/measurements")
            .WithTags("Measurements");

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