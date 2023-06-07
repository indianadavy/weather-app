using MongoDB.Driver;

namespace WeatherApp.Factories;

public interface IDataAccessFactory
{
    IMongoCollection<WeatherForecast> GetMongoCollection();
}