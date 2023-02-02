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
        //Adding Declarative way
        //read from config, kubernetes or local
        services.AddOptions<PubSubOptions>().Bind(configuration.GetSection("PubSub")).Configure(_ =>
        {
            if (_.IsMultiTenant)
            {
                //read config from configuration
                //GET THE DEFAULT PUB SUB PROVIDER 
                //ATTACH CLIENT ID IF MULTI TENANT
                _.Name += "client2";
            }
        });
        return services;
    }
}

public class PubSubOptions
{
    public string Name { get; set; } = string.Empty;
    public QueueProvider QueueProvider { get; set; } = QueueProvider.RABBITMQ;
    public bool IsMultiTenant { get; set; } = false;
    public List<TopicRouteMapping> TopicRouteMapping { get; set; } = new List<TopicRouteMapping>();
}

public enum QueueProvider
{
    RABBITMQ,
    PULSAR,
    REDIS
}

public class TopicRouteMapping
{
    public string QueueName { get; set; } = string.Empty;
    public string MethodRoute { get; set; } = string.Empty;
}
