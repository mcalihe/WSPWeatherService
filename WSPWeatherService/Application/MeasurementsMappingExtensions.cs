using Riok.Mapperly.Abstractions;
using WSPWeatherService.Api.Models;
using WSPWeatherService.Persistence.Models;

namespace WSPWeatherService.Application;

[Mapper]
public static partial class MeasurementsMappingExtensions
{
    public static partial MeasurementDto? ToDto(this MeasurementEntity? measurement);
}