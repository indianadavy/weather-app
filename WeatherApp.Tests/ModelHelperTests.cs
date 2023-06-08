using MongoDB.Driver.GeoJsonObjectModel;
using WeatherApp;
using NUnit.Framework;

namespace WeatherApp.Tests;

public class ModelHelperTests
{
    // test MapToWeatherForcast
    [Test]
    public void MapToWeatherForcast_ShouldReturnWeatherForecast_WhenResponseIsSuccessful()
    {
        // Arrange
        var weatherForecastDto = new WeatherForecastDto
        {
            latitude = 13.4050,
            longitude = 52.5200,
            generationtime_ms = 0.12345678,
            utc_offset_seconds = 3600,
            timezone = "Europe/Berlin",
            timezone_abbreviation = "CET",
            elevation = 34.0,
            current_weather = new CurrentWeather { temperature = 15.0 },
            hourly_units = new HourlyUnits { temperature_2m = "°C" },
            hourly = new Hourly { temperature_2m = new List<double> { 15.0 } },
        };

        // Act
        var result = ModelHelper.MapToWeatherForcast(weatherForecastDto);

        // Assert
        Assert.That(result.location.Coordinates.Latitude, Is.EqualTo(weatherForecastDto.latitude));
        Assert.That(result.location.Coordinates.Longitude, Is.EqualTo(weatherForecastDto.longitude));
        Assert.That(result.generationtime_ms, Is.EqualTo(weatherForecastDto.generationtime_ms));
        Assert.That(result.utc_offset_seconds, Is.EqualTo(weatherForecastDto.utc_offset_seconds));
        Assert.That(result.timezone, Is.EqualTo(weatherForecastDto.timezone));
        Assert.That(result.timezone_abbreviation, Is.EqualTo(weatherForecastDto.timezone_abbreviation));
        Assert.That(result.elevation, Is.EqualTo(weatherForecastDto.elevation));
        Assert.That(result.current_weather, Is.EqualTo(weatherForecastDto.current_weather));
        Assert.That(result.hourly_units, Is.EqualTo(weatherForecastDto.hourly_units));
        Assert.That(result.hourly, Is.EqualTo(weatherForecastDto.hourly));
    }
    
    // test MapToDto
    [Test]
    public void MapToDto_ShouldReturnWeatherForecastDto_WhenResponseIsSuccessful()
    {
        var location = new GeoJsonPoint<GeoJson2DGeographicCoordinates>(
            new GeoJson2DGeographicCoordinates(13.4050, 52.5200));
        // Arrange
        var weatherForecast = new WeatherForecast
        {
            location = location,
            generationtime_ms = 0.12345678,
            utc_offset_seconds = 3600,
            timezone = "Europe/Berlin",
            timezone_abbreviation = "CET",
            elevation = 34.0,
            current_weather = new CurrentWeather { temperature = 15.0 },
            hourly_units = new HourlyUnits { temperature_2m = "°C" },
            hourly = new Hourly { temperature_2m = new List<double> { 15.0 } },
        };

        // Act
        var result = ModelHelper.MapToDto(weatherForecast);

        // Assert
        Assert.That(result.latitude, Is.EqualTo(weatherForecast.location.Coordinates.Latitude));
        Assert.That(result.longitude, Is.EqualTo(weatherForecast.location.Coordinates.Longitude));
        Assert.That(result.generationtime_ms, Is.EqualTo(weatherForecast.generationtime_ms));
        Assert.That(result.utc_offset_seconds, Is.EqualTo(weatherForecast.utc_offset_seconds));
        Assert.That(result.timezone, Is.EqualTo(weatherForecast.timezone));
        Assert.That(result.timezone_abbreviation, Is.EqualTo(weatherForecast.timezone_abbreviation));
        Assert.That(result.elevation, Is.EqualTo(weatherForecast.elevation));
        Assert.That(result.current_weather, Is.EqualTo(weatherForecast.current_weather));
        Assert.That(result.hourly_units, Is.EqualTo(weatherForecast.hourly_units));
        Assert.That(result.hourly, Is.EqualTo(weatherForecast.hourly));
    }
}