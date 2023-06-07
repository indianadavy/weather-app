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
        var weatherDataDto = new WeatherForecastDto
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
        var result = ModelHelper.MapToWeatherForcast(weatherDataDto);

        // Assert
        Assert.That(result.location.Coordinates.Latitude, Is.EqualTo(weatherDataDto.latitude));
        Assert.That(result.location.Coordinates.Longitude, Is.EqualTo(weatherDataDto.longitude));
        Assert.That(result.generationtime_ms, Is.EqualTo(weatherDataDto.generationtime_ms));
        Assert.That(result.utc_offset_seconds, Is.EqualTo(weatherDataDto.utc_offset_seconds));
        Assert.That(result.timezone, Is.EqualTo(weatherDataDto.timezone));
        Assert.That(result.timezone_abbreviation, Is.EqualTo(weatherDataDto.timezone_abbreviation));
        Assert.That(result.elevation, Is.EqualTo(weatherDataDto.elevation));
        Assert.That(result.current_weather, Is.EqualTo(weatherDataDto.current_weather));
        Assert.That(result.hourly_units, Is.EqualTo(weatherDataDto.hourly_units));
        Assert.That(result.hourly, Is.EqualTo(weatherDataDto.hourly));
    }
    
    // test MapToDto
    [Test]
    public void MapToDto_ShouldReturnWeatherForecastDto_WhenResponseIsSuccessful()
    {
        var location = new GeoJsonPoint<GeoJson2DGeographicCoordinates>(
            new GeoJson2DGeographicCoordinates(13.4050, 52.5200));
        // Arrange
        var weatherData = new WeatherForecast
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
        var result = ModelHelper.MapToDto(weatherData);

        // Assert
        Assert.That(result.latitude, Is.EqualTo(weatherData.location.Coordinates.Latitude));
        Assert.That(result.longitude, Is.EqualTo(weatherData.location.Coordinates.Longitude));
        Assert.That(result.generationtime_ms, Is.EqualTo(weatherData.generationtime_ms));
        Assert.That(result.utc_offset_seconds, Is.EqualTo(weatherData.utc_offset_seconds));
        Assert.That(result.timezone, Is.EqualTo(weatherData.timezone));
        Assert.That(result.timezone_abbreviation, Is.EqualTo(weatherData.timezone_abbreviation));
        Assert.That(result.elevation, Is.EqualTo(weatherData.elevation));
        Assert.That(result.current_weather, Is.EqualTo(weatherData.current_weather));
        Assert.That(result.hourly_units, Is.EqualTo(weatherData.hourly_units));
        Assert.That(result.hourly, Is.EqualTo(weatherData.hourly));
    }
}