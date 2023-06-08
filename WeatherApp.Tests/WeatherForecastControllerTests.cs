using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Driver.GeoJsonObjectModel;
using Moq;
using NUnit.Framework;
using WeatherApp.Controllers;
using WeatherApp.DataAccess;
using WeatherApp.DataAccess.Interfaces;

namespace WeatherApp.Tests;

[TestFixture]
public class WeatherForecastControllerTests
{
    
    private Mock<IMongoDbCollectionDataAccess> _mockMongoDbDataAccess;
    private Mock<IOpenMeteoDataAccess> _mockOpenMeteoDataAccess;
    private Mock<ILogger<WeatherForecastController>> _mockLogger;
    private WeatherForecast _weatherForecast;
    private WeatherForecastDto _weatherForecastDto;

    [SetUp]
    public void Setup()
    {
        _mockMongoDbDataAccess = new Mock<IMongoDbCollectionDataAccess>();
        _mockOpenMeteoDataAccess = new Mock<IOpenMeteoDataAccess>();
        _mockLogger = new Mock<ILogger<WeatherForecastController>>();
        
        _weatherForecastDto = new WeatherForecastDto
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
        _weatherForecast = new WeatherForecast
        {
            location = new GeoJsonPoint<GeoJson2DGeographicCoordinates>(
                new GeoJson2DGeographicCoordinates(13.4050, 52.5200)),
            generationtime_ms = 0.12345678,
            utc_offset_seconds = 3600,
            timezone = "Europe/Berlin",
            timezone_abbreviation = "CET",
            elevation = 34.0,
            current_weather = new CurrentWeather { temperature = 15.0 },
            hourly_units = new HourlyUnits { temperature_2m = "°C" },
            hourly = new Hourly { temperature_2m = new List<double> { 15.0 } },
        };
    }

    #region GetForecast(string id)
    
    [Test]
    public async Task GetForecastById_ReturnsWeatherForecast_WhenDocumentFound()
    {
        // Arrange
        _mockMongoDbDataAccess.Setup(d => d.GetOneAsync(It.IsAny<string>()))
            .ReturnsAsync(() => _weatherForecastDto);
        var controller = new WeatherForecastController(_mockLogger.Object, _mockMongoDbDataAccess.Object, _mockOpenMeteoDataAccess.Object);

        // Act
        var result = await controller.GetForecast(It.IsAny<string>());
        
        // Assert
        Assert.That(result is OkObjectResult);
        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult.Value, Is.EqualTo(_weatherForecastDto));
        // assert that mongoDb is called once
        _mockMongoDbDataAccess.Verify(d => d.GetOneAsync(It.IsAny<string>()), Times.Once);
    }

    [Test]
    public async Task GetForecastById_ReturnsNotFound()
    {
        // Arrange
        _mockMongoDbDataAccess.Setup(d => d.GetOneAsync(It.IsAny<string>()))
            .ReturnsAsync(() => null);
        var controller = new WeatherForecastController(_mockLogger.Object, _mockMongoDbDataAccess.Object, _mockOpenMeteoDataAccess.Object);

        // Act
        var result = await controller.GetForecast(It.IsAny<string>());
        
        // Assert
        Assert.That(result is NotFoundObjectResult);
        var notFoundResult = result as NotFoundObjectResult;
        Assert.That(notFoundResult.StatusCode, Is.EqualTo(StatusCodes.Status404NotFound));
        // assert that mongoDb is called once
        _mockMongoDbDataAccess.Verify(d => d.GetOneAsync(It.IsAny<string>()), Times.Once);
    }

    #endregion
    
    #region GetForecast(Coordinates coordinates)

    [Test]
    public async Task GetForecast_ReturnsWeatherForecast_WhenDocumentFound()
    {
        // Arrange
        _mockMongoDbDataAccess.Setup(d => d.GetOneAsync(It.IsAny<double>(), It.IsAny<double>()))
            .ReturnsAsync(() => _weatherForecastDto);
        var controller = new WeatherForecastController(_mockLogger.Object, _mockMongoDbDataAccess.Object, _mockOpenMeteoDataAccess.Object);

        // Act
        var result = await controller.GetForecast(new Coordinates {longitude = It.IsAny<double>(), latitude = It.IsAny<double>()});
        
        // Assert
        Assert.That(result is OkObjectResult);
        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult.Value, Is.EqualTo(_weatherForecastDto));
        // assert that open-meteo api is not called
        _mockOpenMeteoDataAccess.Verify(d => d.GetForecast(It.IsAny<double>(), It.IsAny<double>()), Times.Never);
        // assert that mongoDb is called once
        _mockMongoDbDataAccess.Verify(d => d.GetOneAsync(It.IsAny<double>(), It.IsAny<double>()), Times.Once);
        // assert that mongoDb.InsertOneAsync is not called
        _mockMongoDbDataAccess.Verify(d => d.InsertOneAsync(It.IsAny<WeatherForecast>()), Times.Never);
    }

    [Test]
    public async Task GetForecast_ReturnsBadRequest_WhenCoordinatesAreInvalid()
    {
        // Arrange
        var controller = new WeatherForecastController(_mockLogger.Object, _mockMongoDbDataAccess.Object, _mockOpenMeteoDataAccess.Object);
        controller.ModelState.AddModelError("longitude", "Invalid coordinates");

        // Act
        var result = await controller.GetForecast(new Coordinates {longitude = It.IsAny<double>(), latitude = It.IsAny<double>()});

        // Assert
        Assert.That(result is BadRequestObjectResult);
        // assert that open-meteo api is not called
        _mockOpenMeteoDataAccess.Verify(d => d.GetForecast(It.IsAny<double>(), It.IsAny<double>()), Times.Never);
        // assert that mongoDb is not called
        _mockMongoDbDataAccess.Verify(d => d.GetOneAsync(It.IsAny<double>(), It.IsAny<double>()), Times.Never);
        // assert that mongoDb.InsertOneAsync is not called
        _mockMongoDbDataAccess.Verify(d => d.InsertOneAsync(It.IsAny<WeatherForecast>()), Times.Never);
    }
    
    [Test]
    public async Task GetForecast_ReturnsWeatherForecast_WhenDocumentNotFound()
    {
        // Arrange
        _mockMongoDbDataAccess.Setup(d => d.GetOneAsync(It.IsAny<double>(), It.IsAny<double>()))
            .ReturnsAsync(() => null);
        _mockOpenMeteoDataAccess.Setup(d => d.GetForecast(It.IsAny<double>(), It.IsAny<double>()))
            .ReturnsAsync(_weatherForecast);
        var controller = new WeatherForecastController(_mockLogger.Object, _mockMongoDbDataAccess.Object, _mockOpenMeteoDataAccess.Object);

        // Act
        var result = await controller.GetForecast(new Coordinates {longitude = It.IsAny<double>(), latitude = It.IsAny<double>()});
        
        // Assert
        Assert.That(result is OkObjectResult);
        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);

        // assert that open-meteo api is called once
        _mockOpenMeteoDataAccess.Verify(d => d.GetForecast(It.IsAny<double>(), It.IsAny<double>()), Times.Once);
        // assert that mongoDb.GetOneAsync is called once
        _mockMongoDbDataAccess.Verify(d => d.GetOneAsync(It.IsAny<double>(), It.IsAny<double>()), Times.Once);
        // assert that mongoDb.InsertOneAsync is called once
        _mockMongoDbDataAccess.Verify(d => d.InsertOneAsync(It.IsAny<WeatherForecast>()), Times.Once);
    }

    #endregion

    #region AddForecast(Coordinates coordinates)
    
    [Test]
public async Task AddForecast_ReturnsCreatedAtActionResult()
    {
        // Arrange
        _mockOpenMeteoDataAccess.Setup(d => d.GetForecast(It.IsAny<double>(), It.IsAny<double>()))
            .ReturnsAsync(_weatherForecast);
        _mockMongoDbDataAccess.Setup(d => d.InsertOneAsync(It.IsAny<WeatherForecast>()));
        
        var controller = new WeatherForecastController(_mockLogger.Object, _mockMongoDbDataAccess.Object, _mockOpenMeteoDataAccess.Object);

        // Act
        var result = await controller.AddForecast(new Coordinates {longitude = It.IsAny<double>(), latitude = It.IsAny<double>()});
        
        // Assert
        Assert.That(result, Is.InstanceOf<CreatedAtActionResult>());
        var objectResult = result as ObjectResult;
        Assert.That(objectResult.StatusCode, Is.EqualTo(StatusCodes.Status201Created));
        
        // assert that open-meteo api is not called
        _mockOpenMeteoDataAccess.Verify(d => d.GetForecast(It.IsAny<double>(), It.IsAny<double>()), Times.Once);
        // assert that mongoDb.InsertOneAsync is not called
        _mockMongoDbDataAccess.Verify(d => d.InsertOneAsync(It.IsAny<WeatherForecast>()), Times.Once);
    }
    
    // test that openMeteo returns null and we return Problem with status code 500
    [Test]
    public async Task AddForecast_ReturnsProblem_WhenOpenMeteoReturnsNull()
    {
        // Arrange
        _mockOpenMeteoDataAccess.Setup(d => d.GetForecast(It.IsAny<double>(), It.IsAny<double>()))
            .ReturnsAsync(() => null);
        _mockMongoDbDataAccess.Setup(d => d.InsertOneAsync(It.IsAny<WeatherForecast>()));
        
        var controller = new WeatherForecastController(_mockLogger.Object, _mockMongoDbDataAccess.Object, _mockOpenMeteoDataAccess.Object);

        // Act
        var result = await controller.AddForecast(new Coordinates {longitude = It.IsAny<double>(), latitude = It.IsAny<double>()});
        
        // Assert
        Assert.That(result, Is.InstanceOf<ObjectResult>());
        var objectResult = result as ObjectResult;
        Assert.That(objectResult.StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));

        // assert that open-meteo api is called once
        _mockOpenMeteoDataAccess.Verify(d => d.GetForecast(It.IsAny<double>(), It.IsAny<double>()), Times.Once);
        // assert that mongoDb.InsertOneAsync is not called
        _mockMongoDbDataAccess.Verify(d => d.InsertOneAsync(It.IsAny<WeatherForecast>()), Times.Never);
    }

    #endregion
    
    
}