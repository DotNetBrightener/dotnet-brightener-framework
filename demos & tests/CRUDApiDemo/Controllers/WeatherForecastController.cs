using DotNetBrightener.WebApi.GenericCRUD.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace CRUDApiDemo.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly WeatherForecast[] Summaries = new[]
    {
        new WeatherForecast
        {
            Summary = "Freezing, 1"
        },
        new WeatherForecast
        {
            Summary = "Bracing, 2"
        },
        new WeatherForecast
        {
            Summary = "Chilly, 3"
        },
        new WeatherForecast
        {
            Summary = "Cool"
        },
        new WeatherForecast
        {
            Summary = "Mild"
        },
        new WeatherForecast
        {
            Summary = "Warm"
        },
    };

    private readonly ILogger<WeatherForecastController> _logger;

    public WeatherForecastController(ILogger<WeatherForecastController> logger)
    {
        _logger = logger;
    }

    [HttpGet(Name = "GetWeatherForecast")]
    public async Task<IEnumerable<WeatherForecast>> Get()
    {
        var deepPropertiesSearchFilters = Request.Query.ToDictionary(_ => _.Key,
                                                                     _ => _.Value.ToString());

        var entitiesQuery = await Summaries.AsQueryable()
                                           .ApplyDeepFilters(deepPropertiesSearchFilters);

        return entitiesQuery.ToArray();
    }
}