using MongoDB.Driver;
using WeatherApp;
using WeatherApp.DataAccess;
using WeatherApp.DataAccess.Interfaces;

namespace WeatherApp.Factories;

public class DataAccessFactory : IDataAccessFactory
{
	private readonly IConfiguration _configuration;
	private readonly IHttpClientFactory _httpClientFactory;

	public DataAccessFactory(IConfiguration configuration, IHttpClientFactory httpClientFactory)
	{
		_configuration = configuration;
		_httpClientFactory = httpClientFactory;
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

	public IOpenMeteoDataAccess GetOpenMeteoDataAccess(IServiceProvider sp)
	{
		// read from environment variables, appsettings or default
		var baseUrl = _configuration.GetValue<string>("OPEN_METEO_BASE_URL") ?? "https://api.open-meteo.com/v1/forecast";
		var httpClient = _httpClientFactory.CreateClient();
		var logger = sp.GetRequiredService<ILogger<OpenMeteoDataAccess>>();
		
		// create the data access object
		var openMeteoDataAccess = new OpenMeteoDataAccess(logger, httpClient, baseUrl);
		
		return openMeteoDataAccess;
	}
}
