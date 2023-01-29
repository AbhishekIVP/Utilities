using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.Metrics;
using ivp.edm.apm;

namespace OpenTelemetrySample.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };
    // private readonly Meter meter;
    // private readonly Observability? _apmHelper;
    // private readonly Counter<long>? counter;
    private readonly ILogger<WeatherForecastController> _logger;
    private readonly IConfiguration _configuration;
    private readonly SampleMeters myMeters;
    public WeatherForecastController(ILogger<WeatherForecastController> logger
        , IConfiguration configuration
        // , Observability apmHelper
        , SampleMeters myMeters)
    {
        _logger = logger;
        _configuration = configuration;
        // _apmHelper = apmHelper;
        this.myMeters = myMeters;
        // counter = _apmHelper?.ServiceMeter?.CreateCounter<long>("app.request-counter");
    }

    [HttpGet(Name = "GetWeatherForecast")]
    public IEnumerable<WeatherForecast> Get()
    {
        // using var activity = _apmHelper?.ServiceActivity?.StartActivity("SayHello");
        // activity?.SetTag("foo", 1);
        // activity?.SetTag("bar", "Hello, World!");
        // activity?.SetTag("baz", new int[] { 1, 2, 3 });

        myMeters.BlockInvoked();
        // counter?.Add(1);
        myMeters.WeatherMethodInvoked();

        _logger.LogCritical("Hello Critical!!!");
        _logger.LogError("Hello Error!!!");
        _logger.LogWarning("Hello Warning!!!");
        _logger.LogInformation("Hello Information!!!");
        _logger.LogDebug("Hello Debug!!!");
        _logger.LogTrace("Hello Trace!!!");

        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateTime.Now.AddDays(index),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        })
        ;

    }
}
