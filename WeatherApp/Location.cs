using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;

namespace WeatherApp;

// public class GeoJsonPointDto
// {
//     public ObjectId _id { get; set; }
//     public Location location { get; set; }
// }
//
// public class Location
// {
//     public string? type { get; set; }
//     public CoordinatesDto CoordinatesDto { get; set; }
// }


public class Coordinates
{
    [Required]
    [Range(-90, 90)]
    public double? longitude { get; set; }
    [Required]
    [Range(-90, 90)]
    public double? latitude { get; set; }
}
