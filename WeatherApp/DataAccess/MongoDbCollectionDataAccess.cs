using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GeoJsonObjectModel;
using WeatherApp.DataAccess.Interfaces;

namespace WeatherApp.DataAccess;

public class MongoDbCollectionDataAccess : IMongoDbCollectionDataAccess
{
    private readonly IMongoCollection<WeatherForecast> _collection;

    public MongoDbCollectionDataAccess(IConfiguration configuration)
    {
        // read from environment variables, appsettings or default
        var url = configuration.GetValue<string>("MONGODB_URL") ?? "mongodb://localhost:27017";
        var collection = configuration.GetValue<string>("MONGODB_COLLECTION") ?? "forecasts";
        var database = configuration.GetValue<string>("MONGODB_DATABASE") ?? "weather";

        // create a client and get the collection
        var mongoClient = new MongoClient(url);
        _collection = mongoClient.GetDatabase(database).GetCollection<WeatherForecast>(collection);
    }

    public async Task InsertOneAsync(WeatherForecast weatherForecast)
    {
        await _collection.InsertOneAsync(weatherForecast);
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
    
    // public async Task<bool> UpdateOneAsync(WeatherForecast weatherForecast)
    // {
    //     var result = await _collection.ReplaceOneAsync(
    //         x => x.location.Coordinates.Longitude == weatherForecast.location.Coordinates.Longitude
    //             && x.location.Coordinates.Latitude == weatherForecast.location.Coordinates.Latitude,
    //         weatherForecast);
    //
    //     if (result.IsAcknowledged && result.ModifiedCount > 0)
    //     {
    //         return true; // Document found and modified
    //     }
    //
    //     if (result.IsAcknowledged && result.ModifiedCount == 0)
    //     {
    //         return false; // Document not found
    //     }
    //
    //     throw new Exception("Update operation failed.");
    // }

    public async Task<WeatherForecastDto?> GetOneAsync(double lon, double lat)
    {
        // apply filter based on coordinates
        var point = GeoJson.Point(GeoJson.Geographic(lon, lat));
        var filter = Builders<WeatherForecast>.Filter.GeoIntersects(x => x.location, point);
        var result = await _collection.Find(filter).FirstOrDefaultAsync();

        if (result != null)
        {
            var forecastDto = new WeatherForecastDto
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

            return forecastDto;
        }

        return null;
    }

    public async Task<WeatherForecastDto> GetOneAsync(string id)
    {
        var filter = Builders<WeatherForecast>.Filter.Eq("_id", ObjectId.Parse(id));
        var result = await _collection.Find(filter).FirstOrDefaultAsync();

        if (result != null)
        {
            var forecastDto = new WeatherForecastDto
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

            return forecastDto;
        }

        return null;
    }

    public async Task<List<WeatherForecast>> List()
    {
        var forecasts = await _collection.Find(new BsonDocument()).ToListAsync();
        return forecasts;
    }
}
