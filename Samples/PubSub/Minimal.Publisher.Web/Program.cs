using System.Text.Json.Serialization;
using Dapr.Client;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", async () =>
{
    for (int i = 1; i <= 10; i++)
    {
        var order = new Order(i);
        using var client = new DaprClientBuilder().Build();

        // Publish an event/message using Dapr PubSub
        await client.PublishEventAsync("redispubsub", "orders", order);
        Console.WriteLine("Published data: " + order);

        await Task.Delay(TimeSpan.FromSeconds(1));
    }
});

await app.RunAsync();

public record Order([property: JsonPropertyName("orderId")] int OrderId);