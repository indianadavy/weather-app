using MongoDB.Driver.GeoJsonObjectModel;

namespace WeatherApp;

public class ModelHelper
{
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
    
    public static WeatherForecastDto? MapToDto(WeatherForecast result)
    {
        return new WeatherForecastDto
        {
            _id = result._id.ToString(),
            latitude = result.location.Coordinates.Latitude,
            longitude = result.location.Coordinates.Longitude,
            generationtime_ms = result.generationtime_ms,
            utc_offset_seconds = result.utc_offset_seconds,
            timezone = result.timezone,
            timezone_abbreviation = result.timezone_abbreviation,
            elevation = result.elevation,
            current_weather = result.current_weather,
            hourly_units = result.hourly_units,
            hourly = result.hourly
        };
    }

}