using System.Text.Json;
using ivp.edm.notification;
using Microsoft.AspNetCore.Mvc;

namespace Sample.Sender.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<WeatherForecastController> _logger;
    private readonly CommandManager _commandManager;
    public WeatherForecastController(ILogger<WeatherForecastController> logger, CommandManager commandManager)
    {
        _logger = logger;
        _commandManager = commandManager;
    }

    [HttpGet(Name = "GetWeatherForecast")]
    public async Task Get()
    {
        var wf = Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateTime.Now.AddDays(index),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        })
        .ToArray();

        //TODO:HTTP POST TO SEND MESSAGE instead of calling it directly;
        Console.WriteLine("Sending the message");

        Command _command = _commandManager.CreateCommandInstance();
        _command.AudienceGroupName = "spanhotra";
        _command.Data = new CommandData()
        {
            Subject = "WeatherForecast",
            Body = JsonSerializer.Serialize(wf)
        };
        _command.Event = new EventStore() { Name = "ForecastReceived", Source = "Sample.Sender" };
        _command.TemplateNames = new List<string>() { "Standard Email" };
        _command.IsCritical = true;

        await _commandManager.Execute(_command);
    }
}
