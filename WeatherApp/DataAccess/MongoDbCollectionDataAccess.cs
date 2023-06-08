using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GeoJsonObjectModel;
using WeatherApp.DataAccess.Interfaces;

namespace WeatherApp.DataAccess;

public class MongoDbCollectionDataAccess : IMongoDbCollectionDataAccess
{
    private readonly ILogger<MongoDbCollectionDataAccess> _logger;
    private readonly IMongoCollection<WeatherForecast> _collection;

    public MongoDbCollectionDataAccess(ILogger<MongoDbCollectionDataAccess> logger, IMongoCollection<WeatherForecast> collection)
    {
        _logger = logger;
        _collection = collection;
    }

    public async Task InsertOneAsync(WeatherForecast weatherForecast)
    {
        try
        {
            await _collection.InsertOneAsync(weatherForecast);
        }
        catch (Exception e)
        {
            _logger.LogError("Error writing to the database: {0}", e.Message);
            throw;
        }
    }

    public async Task<bool> DeleteOneAsync(double lon, double lat)
    {
        // create a point
        var point = GeoJson.Point(GeoJson.Geographic(lon, lat));

        // find the document with matching coordinates
        var filter = Builders<WeatherForecast>.Filter.GeoIntersects(x => x.location, point);
        var result = await _collection.DeleteOneAsync(filter);

        if (result.IsAcknowledged && result.DeletedCount > 0)
        {
            return true; // Document found and deleted
        }

        if (result.IsAcknowledged && result.DeletedCount == 0)
        {
            return false; // Document not found
        }

        throw new Exception("Delete operation failed.");
    }

    public async Task<bool> UpdateOneAsync(WeatherForecast weatherForecast)
    {
        var filter = new BsonDocument
        {
            {
                "location.coordinates", new BsonArray(new [] { weatherForecast.location.Coordinates.Longitude, weatherForecast.location.Coordinates.Latitude })
            }
        };
        var result = await _collection.ReplaceOneAsync(filter, weatherForecast);

        if (result.IsAcknowledged && result.ModifiedCount > 0)
        {
            return true; // Document found and modified
        }

        if (result.IsAcknowledged && result.ModifiedCount == 0)
        {
            return false; // Document not found
        }

        throw new Exception("Update operation failed.");
    }

    public async Task<WeatherForecastDto?> GetOneAsync(double lon, double lat)
    {
        // apply filter based on coordinates
        var point = GeoJson.Point(GeoJson.Geographic(lon, lat));
        var filter = Builders<WeatherForecast>.Filter.GeoIntersects(x => x.location, point);
        var result = await _collection.Find(filter).FirstOrDefaultAsync();

        if (result != null)
        {
            return ModelHelper.MapToDto(result);
        }
        return null;
    }

    public async Task<WeatherForecastDto?> GetOneAsync(string id)
    {
        var filter = Builders<WeatherForecast>.Filter.Eq("_id", ObjectId.Parse(id));
        var result = await _collection.Find(filter).FirstOrDefaultAsync();

        if (result != null)
        {
            return ModelHelper.MapToDto(result);
        }

        return null;
    }

    public async Task<List<WeatherForecast>> List()
    {
        var forecasts = await _collection.Find(new BsonDocument()).ToListAsync();
        return forecasts;
    }
}
