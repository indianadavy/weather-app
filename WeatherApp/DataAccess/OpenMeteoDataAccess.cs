using System.Text.Json;
using WeatherApp.DataAccess.Interfaces;

namespace WeatherApp.DataAccess;

public class OpenMeteoDataAccess: IOpenMeteoDataAccess
{
    private readonly HttpClient _client;
    private readonly string _baseUrl;
    private readonly ILogger<OpenMeteoDataAccess> _logger;

    public OpenMeteoDataAccess(ILogger<OpenMeteoDataAccess> logger, HttpClient client, string baseUrl)
    {
        _logger = logger;
        _client = client;
        _baseUrl = baseUrl;
    }
    
    public async Task<WeatherForecast?> GetForecast(double lon, double lat)
    {
        try
        {
            var url = $"{_baseUrl}?latitude={lat}&longitude={lon}&current_weather=true&hourly=temperature_2m,relativehumidity_2m,windspeed_10m";
            var response = await _client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var weatherDataDto = JsonSerializer.Deserialize<WeatherForecastDto>(content);
                if (weatherDataDto is { longitude: not null, latitude: not null })
                {
                    var weatherForecast = WeatherForecast.MapToWeatherForcast(weatherDataDto);
                    return weatherForecast;
                }
            }
            _logger.LogError("Error fetching data from Open-Meteo API: {0}", response.ReasonPhrase);
        }
        catch (HttpRequestException e)
        {
            _logger.LogError("Error fetching data from Open-Meteo API: {0}", e.Message);
        }

        return null;
    }
}