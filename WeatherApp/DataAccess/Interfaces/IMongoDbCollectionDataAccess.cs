using MongoDB.Driver.GeoJsonObjectModel;

namespace WeatherApp.DataAccess.Interfaces;

public interface IMongoDbCollectionDataAccess
{
    Task InsertOneAsync(WeatherForecast weatherForecast);
    Task<bool> DeleteOneAsync(double lon, double lat);
    Task<bool> UpdateOneAsync(WeatherForecast weatherForecast);
    Task<WeatherForecastDto?> GetOneAsync(double lon, double lat);
    Task<WeatherForecastDto> GetOneAsync(string id);
    Task<List<WeatherForecast>> List();
}