using Dapr.Client;
using ivp.edm.validations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ivp.edm.pubsub;
public static class PubSubEntensions
{
    public static IHostBuilder InitializePubSub(this IHostBuilder builder)
    {
        return builder.ConfigureServices((context, collection) =>
        {
            collection.InitializePubSub(context.Configuration);
        });
    }

    public static IServiceCollection InitializePubSub(this IServiceCollection services, IConfiguration configuration)
    {
        ServiceChainGuard.ValidateInstance<DaprClient>(services);

        //read config from configuration
        //GET THE DEFAULT PUB SUB PROVIDER 
        //ATTACH CLIENT ID IF MULTI TENANT
        string pubSubName = "rabbitmq-client2";

        //Adding Declarative way
        //read from config, kubernetes or local
        //
        services.AddOptions<PubSubOptions>().Configure(_ =>
        {
            _.PubSubName = pubSubName;
            _.TopicRouteMapping = new List<TopicRouteMapping>() { new TopicRouteMapping() { QueueName = "edmqueue", MethodRoute = "ProcessMessage" } };
        });
        return services;
    }
}

public class PubSubOptions
{
    public string PubSubName { get; set; } = string.Empty;
    public List<TopicRouteMapping> TopicRouteMapping { get; set; } = new List<TopicRouteMapping>();
}

public class TopicRouteMapping
{
    public string QueueName { get; set; } = string.Empty;
    public string MethodRoute { get; set; } = string.Empty;
}
