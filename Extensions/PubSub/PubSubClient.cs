using Dapr.Client;
using ivp.edm.validations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace ivp.edm.pubsub;

public class PubSubClient
{
    private readonly IConfiguration _configuration;
    private readonly DaprClient _daprClient;
    private readonly IOptions<PubSubOptions> _pubsubOptions;
    public PubSubClient(IConfiguration configuration, DaprClient daprClient, IOptions<PubSubOptions> pubsubOptions)
    {
        _daprClient = daprClient;
        _configuration = configuration;
        _pubsubOptions = pubsubOptions;
    }
    public async Task PublishEventAsync<T>(string queueName, T message)
    {
        ArgumentGuard.NotNullOrEmpty(queueName);
        ArgumentGuard.NotNull<T>(message);

        string pubsubName;
        if (_pubsubOptions.Value.IsMultiTenant)
            //TODO:ATTACH Current Request CLIENT ID IF MULTI TENANT from Tenancy Provider and build pubsub name
            pubsubName = $"{_pubsubOptions.Value.Name}-client2";
        else
            pubsubName = $"{_pubsubOptions.Value.Name}";
        await _daprClient.PublishEventAsync<T>(pubsubName, queueName, message);
    }
}