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
        services.AddOptions<PubSubOptions>().Bind(configuration.GetSection("PubSub")).Configure(_ =>
        {
            if (string.IsNullOrEmpty(_.Name)) _.Name = $"{_.QueueProvider.ToString().ToLower()}";
            if (_.IsMultiTenant)
            {
                switch (_.QueueProvider)
                {
                    case QueueProvider.RABBITMQ:
                    case QueueProvider.REDIS:
                        //TODO:ATTACH Current Request CLIENT ID IF MULTI TENANT from Tenancy Provider
                        _.PubSubName = $"{_.Name}-client2";
                        break;
                    case QueueProvider.PULSAR:
                    default:
                        throw new NotImplementedException($"Multitenancy for {_.QueueProvider} is not implemented.");
                }
            }
            else
                _.PubSubName = _.Name;
        });
        return services;
    }
}

public class PubSubOptions
{
    internal string Name { get; set; } = string.Empty;
    public string PubSubName { get; internal set; } = string.Empty;
    public QueueProvider QueueProvider { get; set; } = QueueProvider.RABBITMQ;
    public bool IsMultiTenant { get; set; } = false;
    public List<TopicRouteMapping> TopicRouteMappings { get; set; } = new List<TopicRouteMapping>();
}

public enum QueueProvider
{
    RABBITMQ,
    PULSAR,
    REDIS
}

public class TopicRouteMapping
{
    public List<string> QueueName { get; set; } = new List<string>();
    public string MethodRoute { get; set; } = string.Empty;
}
