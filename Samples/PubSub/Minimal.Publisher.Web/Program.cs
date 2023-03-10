using System.Text.Json.Serialization;
using ivp.edm.pubsub;
using ivp.edm;

var builder = WebApplication.CreateBuilder(args);

builder.AddExtensions(Enable.PUB_SUB);

var app = builder.Build();

app.MapGet("/", async (PubSubClient pubsubClient) =>
{
    for (int i = 1; i <= 10; i++)
    {
        var order = new Order(i);

        // Publish an event/message using Dapr PubSub
        // await _daprClient.PublishEventAsync(_pubSubOptions.Value.PubSubName, "edmqueue1", order);
        // await _daprClient.PublishEventAsync(_pubSubOptions.Value.PubSubName, "edmqueue2", order);
        // await _daprClient.PublishEventAsync(_pubSubOptions.Value.PubSubName, "edmqueue3", order);

        await pubsubClient.PublishEventAsync("edmqueue1", order);

        Console.WriteLine("Published data: " + order);

        await Task.Delay(TimeSpan.FromSeconds(1));
    }
});

app.Run();

public record Order([property: JsonPropertyName("orderId")] int OrderId);