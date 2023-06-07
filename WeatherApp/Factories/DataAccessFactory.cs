using MongoDB.Driver;
using WeatherApp;
using WeatherApp.DataAccess;
using WeatherApp.DataAccess.Interfaces;

namespace WeatherApp.Factories;

public class DataAccessFactory : IDataAccessFactory
{
	// private readonly IHttpClientFactory _httpClientFactory;
	private readonly IConfiguration _configuration;

	public DataAccessFactory(IConfiguration configuration)
	{
		// _httpClientFactory = httpClientFactory;
		_configuration = configuration;
	}
	
	public IMongoCollection<WeatherForecast> GetMongoCollection()
	{
		// read from environment variables, appsettings or default
		var url = _configuration.GetValue<string>("MONGODB_URL") ?? "mongodb://localhost:27017";
		var collection = _configuration.GetValue<string>("MONGODB_COLLECTION") ?? "forecasts";
		var database = _configuration.GetValue<string>("MONGODB_DATABASE") ?? "weather";
		
		// create a client and get the collection
		var mongoClient = new MongoClient(url);
		var mongoCollection = mongoClient.GetDatabase(database).GetCollection<WeatherForecast>(collection);
		
		return mongoCollection;
	}
}
