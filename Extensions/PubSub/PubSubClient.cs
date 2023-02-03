using Dapr.Client;
using ivp.edm.validations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ivp.edm.pubsub;

public class PubSubClient
{
    private readonly IConfiguration _configuration;
    private readonly DaprClient _daprClient;
    private readonly IOptions<PubSubOptions> _pubsubOptions;
    private readonly ILogger<PubSubClient> _logger;
    public PubSubClient(IConfiguration configuration, DaprClient daprClient, IOptions<PubSubOptions> pubsubOptions, ILogger<PubSubClient> logger)
    {
        _daprClient = daprClient;
        _configuration = configuration;
        _pubsubOptions = pubsubOptions;
        _logger = logger;
    }
    public async Task PublishEventAsync<T>(string queueName, T message)
    {
        ArgumentGuard.NotNullOrEmpty(queueName);
        ArgumentGuard.NotNull<T>(message);

        _logger.LogInformation($"Method Name PublishEventAsync / QueueName {queueName}");

        string pubsubName;
        if (_pubsubOptions.Value.IsMultiTenant)
        {
            string clientName = "client2";
            _logger.LogDebug($"Client Name {clientName}");
            //TODO:ATTACH Current Request CLIENT ID IF MULTI TENANT from Tenancy Provider and build pubsub name
            pubsubName = $"{_pubsubOptions.Value.Name}-{clientName}";
        }
        else
            pubsubName = $"{_pubsubOptions.Value.Name}";
        _logger.LogDebug($"PubSubName {pubsubName}");
        await _daprClient.PublishEventAsync<T>(pubsubName, queueName, message);
    }
}