using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver.GeoJsonObjectModel;

namespace WeatherApp;

public class WeatherForecast
{
    [BsonId]
    public ObjectId _id { get; set; }
    [BsonElement("location")]
    public GeoJsonPoint<GeoJson2DGeographicCoordinates> location { get; set; }
    public double? generationtime_ms { get; set; }
    public int? utc_offset_seconds { get; set; }
    public string? timezone { get; set; }
    public string? timezone_abbreviation { get; set; }
    public double? elevation { get; set; }
    public CurrentWeather current_weather { get; set; }
    public HourlyUnits hourly_units { get; set; }
    public Hourly hourly { get; set; }
    
    public static WeatherForecast MapToWeatherForcast(WeatherForecastDto weatherDataDto)
    {
        var weatherForecast = new WeatherForecast
        {
            location = new GeoJsonPoint<GeoJson2DGeographicCoordinates>(
                new GeoJson2DGeographicCoordinates(weatherDataDto.longitude.Value, weatherDataDto.latitude.Value)
            ),
            generationtime_ms = weatherDataDto.generationtime_ms,
            utc_offset_seconds = weatherDataDto.utc_offset_seconds,
            timezone = weatherDataDto.timezone,
            timezone_abbreviation = weatherDataDto.timezone_abbreviation,
            elevation = weatherDataDto.elevation,
            current_weather = weatherDataDto.current_weather,
            hourly_units = weatherDataDto.hourly_units,
            hourly = weatherDataDto.hourly
        };
        return weatherForecast;
    }
}

public class WeatherForecastDto
{
    public string? _id { get; set; }
    public double? latitude { get; set; }
    public double? longitude { get; set; }
    public double? generationtime_ms { get; set; }
    public int? utc_offset_seconds { get; set; }
    public string? timezone { get; set; }
    public string? timezone_abbreviation { get; set; }
    public double? elevation { get; set; }
    public CurrentWeather current_weather { get; set; }
    public HourlyUnits hourly_units { get; set; }
    public Hourly hourly { get; set; }
}

public class CurrentWeather
{
    public double? temperature { get; set; }
    public double? windspeed { get; set; }
    public double? winddirection { get; set; }
    public int? weathercode { get; set; }
    public int? is_day { get; set; }
    public string? time { get; set; }
}

public class HourlyUnits
{
    public string? time { get; set; }
    public string? temperature_2m { get; set; }
    public string? relativehumidity_2m { get; set; }
    public string? windspeed_10m { get; set; }
}

public class Hourly
{
    public List<string>? time { get; set; }
    public List<double>? temperature_2m { get; set; }
}
