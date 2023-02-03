using System.Text.Json.Serialization;
using Dapr.Client;
using ivp.edm.pubsub;
using ivp.edm;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.AddExtensions(Enable.PUB_SUB);

var app = builder.Build();

app.MapGet("/", async (DaprClient _daprClient, IOptions<PubSubOptions> _pubSubOptions) =>
{
    for (int i = 1; i <= 10; i++)
    {
        var order = new Order(i);

        // Publish an event/message using Dapr PubSub
        await _daprClient.PublishEventAsync(_pubSubOptions.Value.Name, "edmqueue1", order);
        await _daprClient.PublishEventAsync(_pubSubOptions.Value.Name, "edmqueue2", order);
        Console.WriteLine("Published data: " + order);

        await Task.Delay(TimeSpan.FromSeconds(1));
    }
});

app.Run();

public record Order([property: JsonPropertyName("orderId")] int OrderId);