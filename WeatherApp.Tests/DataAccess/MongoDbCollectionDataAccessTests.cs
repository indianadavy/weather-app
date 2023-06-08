using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Moq;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Driver.GeoJsonObjectModel;
using WeatherApp.DataAccess;

namespace WeatherApp.Tests.DataAccess;

[TestFixture]
public class MongoDbCollectionDataAccessTests
{
    private Mock<IMongoCollection<WeatherForecast>> _mockCollection;
    private MongoDbCollectionDataAccess _dao;
    private Mock<ILogger<MongoDbCollectionDataAccess>> _loggerMock;

    [SetUp]
    public void Setup()
    {
        _loggerMock = new Mock<ILogger<MongoDbCollectionDataAccess>>();
        _mockCollection = new Mock<IMongoCollection<WeatherForecast>>();
        _dao = new MongoDbCollectionDataAccess(_loggerMock.Object, _mockCollection.Object);
    }

    #region InsertOneAsync

    [Test]
    public async Task InsertOneAsync_CallsInsertOneAsyncOnCollection()
    {
        var weatherForecast = new WeatherForecast();

        _mockCollection.Setup(c => c.InsertOneAsync(weatherForecast, null, default(CancellationToken)))
            .Returns(Task.CompletedTask);

        await _dao.InsertOneAsync(weatherForecast);

        _mockCollection.Verify(c => c.InsertOneAsync(weatherForecast, null, default(CancellationToken)), Times.Once);
    }

    #endregion

    #region DeleteOneAsync

    [Test]
    public async Task DeleteOneAsync_ReturnsTrue_WhenDocumentDeleted()
    {
        // create result with 1 deleted document
        var deleteResult = new DeleteResult.Acknowledged(1);
        _mockCollection.Setup(c => c.DeleteOneAsync(It.IsAny<FilterDefinition<WeatherForecast>>(), default(CancellationToken))).ReturnsAsync(deleteResult);

        var result = await _dao.DeleteOneAsync(12.34, 56.78);

        Assert.That(result, Is.True);
    }
    
    [Test]
    public async Task DeleteOneAsync_ReturnsFalse_WhenDocumentNotFound()
    {
        // create result with 0 deleted documents
        var deleteResult = new DeleteResult.Acknowledged(0);
        
        _mockCollection.Setup(c => c.DeleteOneAsync(It.IsAny<FilterDefinition<WeatherForecast>>(), default(CancellationToken))).ReturnsAsync(deleteResult);

        var result = await _dao.DeleteOneAsync(12.34, 56.78);

        Assert.That(result, Is.False);
    }
    
    [Test]
    public void DeleteOneAsync_ThrowsException_WhenDeleteFails()
    {
        // create result with 0 deleted documents
        var deleteResult = new DeleteResult.Acknowledged(0);
        
        _mockCollection.Setup(c => c.DeleteOneAsync(It.IsAny<FilterDefinition<WeatherForecast>>(), default(CancellationToken))).ThrowsAsync(new Exception("Delete operation failed."));

        Assert.ThrowsAsync<Exception>(async () => await _dao.DeleteOneAsync(12.34, 56.78));
    }

    #endregion

    #region UpdateOneAsync

    [Test]
    public async Task UpdateOneAsync_CallsReplaceOneAsyncOnCollection()
    {
        var weatherForecast = new WeatherForecast();
        weatherForecast.location = new GeoJsonPoint<GeoJson2DGeographicCoordinates>(
            new GeoJson2DGeographicCoordinates(It.IsAny<double>(), It.IsAny<double>()));
        
        var acknowledgedResult = new ReplaceOneResult.Acknowledged(1, 1, new BsonDocument());

        _mockCollection.Setup(c => c.ReplaceOneAsync(
                It.IsAny<FilterDefinition<WeatherForecast>>(), 
                weatherForecast,
                It.IsAny<ReplaceOptions>(), 
                default(CancellationToken))
            )
            .ReturnsAsync( acknowledgedResult );

        var result = await _dao.UpdateOneAsync(weatherForecast);
        
        Assert.That(result, Is.True);

        _mockCollection.Verify(c => c.ReplaceOneAsync(
            It.IsAny<FilterDefinition<WeatherForecast>>(),
            weatherForecast,
            It.IsAny<ReplaceOptions>(),
            default(CancellationToken)), Times.Once);
    }

    #endregion
}
