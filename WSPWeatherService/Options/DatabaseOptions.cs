using System.ComponentModel.DataAnnotations;

namespace WSPWeatherService.Options;

public class DatabaseOptions
{
    public const string SectionName = "DatabaseOptions";
    
    [Required]
    public required string SqlDataConnectionString { get; set; }
    [Required]
    public required string HangfireConnectionString { get; set; }
}