using System.Diagnostics;
using Medallion.Threading;
using Microsoft.AspNetCore.Mvc;

namespace Locking.Web.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<WeatherForecastController> _logger;
    private readonly IDistributedLockProvider _lockProvider;

    public WeatherForecastController(ILogger<WeatherForecastController> logger, IDistributedLockProvider lockProvider)
    {
        _logger = logger;
        _lockProvider = lockProvider;
    }

    [HttpGet(Name = "GetWeatherForecast")]
    public async IAsyncEnumerable<WeatherForecast> Get()
    {
        _logger.LogDebug($"{Activity.Current?.TraceId} is trying to acquire the lock.");
        await using (await this._lockProvider.AcquireLockAsync("Weather"))
        {
            _logger.LogDebug($"{Activity.Current?.TraceId} acquired the lock.");
            // do 
            await Task.Delay(10000);
            await foreach (int index in RangeAsync(1, 5))
            {
                yield return new WeatherForecast
                {
                    Date = DateTime.Now.AddDays(index),
                    TemperatureC = Random.Shared.Next(-20, 55),
                    Summary = Summaries[Random.Shared.Next(Summaries.Length)]
                };
            }
            _logger.LogDebug($"{Activity.Current?.TraceId} released the lock.");
        }
    }

    async IAsyncEnumerable<int> RangeAsync(int start, int count)
    {
        for (int i = 0; i < count; i++)
        {
            await Task.Delay(i);
            yield return start + i;
        }
    }
}
