using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver.GeoJsonObjectModel;
using Moq;
using NUnit.Framework;
using WeatherApp.Controllers;
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
            _id = ObjectId.GenerateNewId().ToString(),
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
            _id = ObjectId.GenerateNewId(),
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
    
    #region UpdateLastForecast(Coordinates coordinates)
    
    [Test]
    // test document updated
    public async Task UpdateLastestForecast_ReturnsOk_WhenDocumentUpdated()
    {
        // Arrange
        _mockOpenMeteoDataAccess.Setup(d => d.GetForecast(It.IsAny<double>(), It.IsAny<double>()))
            .ReturnsAsync(_weatherForecast);
        _mockMongoDbDataAccess.Setup(d => d.GetOneAsync(It.IsAny<double>(), It.IsAny<double>()))
            .ReturnsAsync(_weatherForecastDto);
        _mockMongoDbDataAccess.Setup(d => d.UpdateOneAsync(It.IsAny<WeatherForecast>()))
            .ReturnsAsync(true);
        
        var controller = new WeatherForecastController(_mockLogger.Object, _mockMongoDbDataAccess.Object, _mockOpenMeteoDataAccess.Object);

        // Act
        var result = await controller.UpdateLatestForecast(new Coordinates {longitude = It.IsAny<double>(), latitude = It.IsAny<double>()});
        
        // Assert
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var objectResult = result as ObjectResult;
        Assert.That(objectResult.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
        
        // assert that open-meteo api is not called
        _mockOpenMeteoDataAccess.Verify(d => d.GetForecast(It.IsAny<double>(), It.IsAny<double>()), Times.Once);
        // assert that mongoDb.GetOneAsync is called once
        _mockMongoDbDataAccess.Verify(d => d.GetOneAsync(It.IsAny<double>(), It.IsAny<double>()), Times.Once);
        // assert that mongoDb.UpdateOneAsync is called once
        _mockMongoDbDataAccess.Verify(d => d.UpdateOneAsync(It.IsAny<WeatherForecast>()), Times.Once);
    }
    
    [Test]
    public async Task UpdateLastestForecast_ReturnsBadRequest_WhenDocumentNotFound()
    {
        // Arrange
        _mockOpenMeteoDataAccess.Setup(d => d.GetForecast(It.IsAny<double>(), It.IsAny<double>()))
            .ReturnsAsync(_weatherForecast);
        _mockMongoDbDataAccess.Setup(d => d.GetOneAsync(It.IsAny<double>(), It.IsAny<double>()))
            .ReturnsAsync(() => null);
        _mockMongoDbDataAccess.Setup(d => d.UpdateOneAsync(It.IsAny<WeatherForecast>()))
            .ReturnsAsync(true);
        
        var controller = new WeatherForecastController(_mockLogger.Object, _mockMongoDbDataAccess.Object, _mockOpenMeteoDataAccess.Object);

        // Act
        var result = await controller.UpdateLatestForecast(new Coordinates {longitude = It.IsAny<double>(), latitude = It.IsAny<double>()});
        
        // Assert
        Assert.That(result, Is.InstanceOf<NotFoundResult>());
        var objectResult = result as NotFoundResult;
        Assert.That(objectResult.StatusCode, Is.EqualTo(StatusCodes.Status404NotFound));
        
        // assert that open-meteo api is not called
        _mockOpenMeteoDataAccess.Verify(d => d.GetForecast(It.IsAny<double>(), It.IsAny<double>()), Times.Never);
        // assert that mongoDb.GetOneAsync is called once
        _mockMongoDbDataAccess.Verify(d => d.GetOneAsync(It.IsAny<double>(), It.IsAny<double>()), Times.Once);
        // assert that mongoDb.UpdateOneAsync is not called
        _mockMongoDbDataAccess.Verify(d => d.UpdateOneAsync(It.IsAny<WeatherForecast>()), Times.Never);
    }
    
    [Test]
    public async Task UpdateLastestForecast_ReturnsProblem_WhenOpenMeteoReturnsNull()
    {
        // Arrange
        _mockOpenMeteoDataAccess.Setup(d => d.GetForecast(It.IsAny<double>(), It.IsAny<double>()))
            .ReturnsAsync(() => null);
        _mockMongoDbDataAccess.Setup(d => d.GetOneAsync(It.IsAny<double>(), It.IsAny<double>()))
            .ReturnsAsync(_weatherForecastDto);
        _mockMongoDbDataAccess.Setup(d => d.UpdateOneAsync(It.IsAny<WeatherForecast>()))
            .ReturnsAsync(true);
        
        var controller = new WeatherForecastController(_mockLogger.Object, _mockMongoDbDataAccess.Object, _mockOpenMeteoDataAccess.Object);

        // Act
        var result = await controller.UpdateLatestForecast(new Coordinates {longitude = It.IsAny<double>(), latitude = It.IsAny<double>()});
        
        // Assert
        Assert.That(result, Is.InstanceOf<ObjectResult>());
        var objectResult = result as ObjectResult;
        Assert.That(objectResult.StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
        
        // assert that open-meteo api is called once
        _mockOpenMeteoDataAccess.Verify(d => d.GetForecast(It.IsAny<double>(), It.IsAny<double>()), Times.Once);
        // assert that mongoDb.GetOneAsync is not called
        _mockMongoDbDataAccess.Verify(d => d.GetOneAsync(It.IsAny<double>(), It.IsAny<double>()), Times.Once);
        // assert that mongoDb.UpdateOneAsync is not called
        _mockMongoDbDataAccess.Verify(d => d.UpdateOneAsync(It.IsAny<WeatherForecast>()), Times.Never);
    }
    
    #endregion
    
    #region DeleteForecast(Coordinates coordinates)
    
    [Test]
    public async Task DeleteForecast_ReturnsNoContent_WhenDocumentDeleted()
    {
        // Arrange
        _mockMongoDbDataAccess.Setup(d => d.DeleteOneAsync(It.IsAny<double>(), It.IsAny<double>()))
            .ReturnsAsync(true);
        
        var controller = new WeatherForecastController(_mockLogger.Object, _mockMongoDbDataAccess.Object, _mockOpenMeteoDataAccess.Object);

        // Act
        var result = await controller.DeleteForecast(new Coordinates {longitude = It.IsAny<double>(), latitude = It.IsAny<double>()});
        
        // Assert
        Assert.That(result, Is.InstanceOf<NoContentResult>());
        var objectResult = result as NoContentResult;
        Assert.That(objectResult.StatusCode, Is.EqualTo(StatusCodes.Status204NoContent));
        
        // assert that mongoDb.DeleteOneAsync is called once
        _mockMongoDbDataAccess.Verify(d => d.DeleteOneAsync(It.IsAny<double>(), It.IsAny<double>()), Times.Once);
    }
    
    // test for when document is not found
    [Test]
    public async Task DeleteForecast_ReturnsNotFound_WhenDocumentNotFound()
    {
        // Arrange
        _mockMongoDbDataAccess.Setup(d => d.DeleteOneAsync(It.IsAny<double>(), It.IsAny<double>()))
            .ReturnsAsync(false);
        
        var controller = new WeatherForecastController(_mockLogger.Object, _mockMongoDbDataAccess.Object, _mockOpenMeteoDataAccess.Object);

        // Act
        var result = await controller.DeleteForecast(new Coordinates {longitude = It.IsAny<double>(), latitude = It.IsAny<double>()});
        
        // Assert
        Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
        var objectResult = result as NotFoundObjectResult;
        Assert.That(objectResult.StatusCode, Is.EqualTo(StatusCodes.Status404NotFound));
        
        // assert that mongoDb.DeleteOneAsync is called once
        _mockMongoDbDataAccess.Verify(d => d.DeleteOneAsync(It.IsAny<double>(), It.IsAny<double>()), Times.Once);
    }
    
    #endregion
    
    #region GetForecasts
    
    [Test]
    public async Task GetForecasts_ReturnsOk_WhenDocumentsFound()
    {
        // Arrange
        var resultList = new List<WeatherForecast>
        {
            _weatherForecast,
            _weatherForecast
        };

        _mockMongoDbDataAccess.Setup(d => d.List())
            .ReturnsAsync(resultList);

        var controller = new WeatherForecastController(_mockLogger.Object, _mockMongoDbDataAccess.Object, _mockOpenMeteoDataAccess.Object);

        // Act
        var result = await controller.GetForecasts();
        
        // Assert
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var objectResult = result as OkObjectResult;
        Assert.That(objectResult.StatusCode, Is.EqualTo(StatusCodes.Status200OK));

        var weatherForecasts = objectResult.Value as IEnumerable<ObjectIdCoordinates>;
        Assert.That(weatherForecasts?.ToList().Count, Is.EqualTo(2));
        
        // assert that mongoDb.GetAllAsync is called once
        _mockMongoDbDataAccess.Verify(d => d.List(), Times.Once);
    }
    
    #endregion
}