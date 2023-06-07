using NUnit.Framework;
using Moq;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using System.Net;
using Moq.Protected;
using WeatherApp.DataAccess;

namespace WeatherApp.Tests.DataAccess;

[TestFixture]
public class OpenMeteoDataAccessTests
{
    private Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private HttpClient _httpClient;
    private Mock<ILogger<OpenMeteoDataAccess>> _loggerMock;

    [SetUp]
    public void Setup()
    {
        _loggerMock = new Mock<ILogger<OpenMeteoDataAccess>>();
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("https://api.open-meteo.com/v1/forecast"),
        };
    }

    [Test]
    public async Task GetForecast_ShouldReturnWeatherForecast_WhenResponseIsSuccessful()
    {
        // Arrange
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(new WeatherForecastDto
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
            })),
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        var openMeteoDataAccess = new OpenMeteoDataAccess(_loggerMock.Object, _httpClient, _httpClient.BaseAddress.AbsoluteUri);

        // Act
        var result = await openMeteoDataAccess.GetForecast(It.IsAny<double>(), It.IsAny<double>());
        
        // Assert
        Assert.DoesNotThrowAsync(() => openMeteoDataAccess.GetForecast(It.IsAny<double>(), It.IsAny<double>()));
        Assert.That(result, Is.Not.Null);
    }
    
    [Test]
    public async Task GetForecast_ShouldReturnNull_WhenResponseIsBadRequest()
    {
        // Arrange
        var httpRequestException = new HttpRequestException("An error occurred.");

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(httpRequestException);

        var openMeteoDataAccess = new OpenMeteoDataAccess(_loggerMock.Object, _httpClient, _httpClient.BaseAddress.AbsoluteUri);

        // Act
        var result = await openMeteoDataAccess.GetForecast(It.IsAny<double>(), It.IsAny<double>());
        
        // Assert
        Assert.DoesNotThrow(() => openMeteoDataAccess.GetForecast(It.IsAny<double>(), It.IsAny<double>()));
        Assert.That(result, Is.Null);
        _loggerMock.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
            Times.AtLeastOnce());
    }
}
