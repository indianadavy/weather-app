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
}