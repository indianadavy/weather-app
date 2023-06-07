using System.Text.Json;
using MongoDB.Bson;
using MongoDB.Driver.GeoJsonObjectModel;
using WeatherApp.Controllers;
using WeatherApp.DataAccess.Interfaces;

namespace WeatherApp.DataAccess;

public class OpenMeteoDataAccess: IOpenMeteoDataAccess
{
    private static readonly HttpClient Client = new HttpClient();
    private static string _baseUrl;
    private readonly ILogger<OpenMeteoDataAccess> _logger;

    public OpenMeteoDataAccess(ILogger<OpenMeteoDataAccess> logger, IConfiguration configuration)
    {
        _logger = logger;   
        _baseUrl = configuration.GetValue<string>("OPEN_METEO_BASE_URL") ?? "https://api.open-meteo.com/v1/forecast";
    }
    
    public async Task<WeatherForecast?> GetForecast(double lon, double lat)
    {
        try
        {
            var url = $"{_baseUrl}?latitude={lat}&longitude={lon}&current_weather=true&hourly=temperature_2m,relativehumidity_2m,windspeed_10m";
            var response = await Client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var weatherDataDto = JsonSerializer.Deserialize<WeatherForecastDto>(content);
                if (weatherDataDto != null  && weatherDataDto.longitude.HasValue && weatherDataDto.latitude.HasValue)
                {
                    var weatherForecast = WeatherForecast.MapToWeatherForcast(weatherDataDto);
                    return weatherForecast;
                }
            }
        }
        catch (HttpRequestException e)
        {
            _logger.LogError("Error fetching data from Open-Meteo API: {0}", e.Message);
        }

        return null;
    }
}