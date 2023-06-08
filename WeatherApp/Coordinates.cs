using System.ComponentModel.DataAnnotations;

namespace WeatherApp;

public class Coordinates
{
    [Required]
    [Range(-90, 90)]
    public double? longitude { get; set; }
    [Required]
    [Range(-90, 90)]
    public double? latitude { get; set; }
}
