using MongoDB.Bson;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using WeatherApp.DataAccess.Interfaces;

namespace WeatherApp.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private readonly ILogger _logger;
    private readonly IOpenMeteoDataAccess _openMeteo;
    private readonly IMongoDbCollectionDataAccess _mongoDb;

    public WeatherForecastController(ILogger<WeatherForecastController> logger, IMongoDbCollectionDataAccess mongoDb, IOpenMeteoDataAccess openMeteo)
    {
        _logger = logger;
        _mongoDb = mongoDb;
        _openMeteo = openMeteo;
    }

    /// <summary>
    /// Gets the weather forecast for the given coordinates from the database by id.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetForecast(string id)
    {
        var forecast = await _mongoDb.GetOneAsync(id);
        if (forecast != null)
        {
            return Ok(forecast);
        }
        
        return BadRequest("Unable to fetch weather");
    }
    
    /// <summary>
    /// Gets the weather forecast for the given coordinates from the database by longitude and latitude.
    /// </summary>
    /// <param name="coordinates"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<IActionResult> GetForecast([FromQuery] Coordinates coordinates)
    {
        // validate coordinates
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        var forecastDto = await _mongoDb.GetOneAsync(coordinates.longitude.Value, coordinates.latitude.Value);
        // if forecast doesn't exist in the database, try to fetch from Open-Meteo API then add to database
        if (forecastDto == null)
        {
            var forecast = await _openMeteo.GetForecast(coordinates.longitude.Value, coordinates.latitude.Value);
            if (forecast != null)
            {
                await _mongoDb.InsertOneAsync(forecast);
                forecastDto = ModelHelper.MapToDto(forecast);
            }
            else
            {
                return Problem(detail: "An error occured", statusCode: StatusCodes.Status500InternalServerError );
            }
        }

        if (forecastDto != null)
        {
            return Ok(forecastDto);
        }

        return BadRequest("Unable to fetch weather");
    }

    /// <summary>
    /// Gets the latest weather forecast for the given coordinates from Open-Meteo API and stores it in the database.
    /// </summary>
    /// <param name="coordinates"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<IActionResult> AddForecast([FromBody] Coordinates coordinates)
    {
        // validate coordinates using ModelState. 
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var weatherData = await _openMeteo.GetForecast(coordinates.longitude.Value, coordinates.latitude.Value);
        if (weatherData != null)
        {
            await _mongoDb.InsertOneAsync(weatherData);

            var response = new { id = weatherData._id.ToString() };

            // return 201 with location of newly created resource. i.e. /weatherforecast/{id}
            return CreatedAtAction("GetForecast", new { id = weatherData._id }, response);
        }

        return Problem(detail: "An error occured", statusCode: StatusCodes.Status500InternalServerError );
    }
    
    [HttpPut]
    public async Task<IActionResult> UpdateLatestForecast([FromQuery] Coordinates coordinates)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        var latestWeatherForecast = await _openMeteo.GetForecast(coordinates.longitude.Value, coordinates.latitude.Value);
        var existingWeatherForecast = await _mongoDb.GetOneAsync(coordinates.longitude.Value, coordinates.latitude.Value);
        
        // if both documents exist, update the existing document with the latest forecast
        // make sure latest has the same _id as existing before updating

        if (latestWeatherForecast != null && existingWeatherForecast != null)
        {
            latestWeatherForecast._id = new ObjectId(existingWeatherForecast._id);
            var updatedResult = await _mongoDb.UpdateOneAsync(latestWeatherForecast);
            if (updatedResult)
            {
                return Ok(latestWeatherForecast);
            }
        }

        return BadRequest("Update failed");
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteForecast([FromBody] Coordinates coordinates)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        var deletedResult = await _mongoDb.DeleteOneAsync(coordinates.longitude.Value, coordinates.latitude.Value);
        
        if (deletedResult)
        {
            return NoContent();
        }

        _logger.LogWarning($"Document not found: lon={coordinates.longitude}, lat={coordinates.latitude}");
        return NotFound("Document not found");
    }
    
    [HttpGet("all")]
    public async Task<IActionResult> GetForecasts()
    {
        var dataPoints = await _mongoDb.List();
        
        // only return id, longitude and latitude
        var result = dataPoints.Select(x => new { id = x._id.ToString(), x.location.Coordinates.Longitude, x.location.Coordinates.Latitude });
        
        return Ok(result);
    }
}
