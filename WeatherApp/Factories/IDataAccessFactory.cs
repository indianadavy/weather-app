using MongoDB.Driver;
using WeatherApp.DataAccess;
using WeatherApp.DataAccess.Interfaces;

namespace WeatherApp.Factories;

public interface IDataAccessFactory
{
    IMongoCollection<WeatherForecast> GetMongoCollection();
    IOpenMeteoDataAccess GetOpenMeteoDataAccess(IServiceProvider sp);
}