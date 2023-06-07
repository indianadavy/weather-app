namespace WeatherApp.DataAccess.Interfaces;

public interface IOpenMeteoDataAccess
{
    Task<WeatherForecast?> GetForecast(double lon, double lat);
}