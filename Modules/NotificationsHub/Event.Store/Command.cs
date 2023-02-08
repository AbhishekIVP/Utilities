using ivp.edm.pubsub;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ivp.edm.notification;
public class CommandManager
{
    private readonly PubSubClient _pubSubClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<CommandManager> _logger;
    public CommandManager(PubSubClient pubsubClient, ILogger<CommandManager> logger, IConfiguration configuration)
    {
        _pubSubClient = pubsubClient;
        _logger = logger;
        _configuration = configuration;
    }

    public Command CreateCommandInstance() => new Command();

    public async Task Execute(Command command)
    {
        try
        {
            await _pubSubClient.PublishEventAsync(_configuration["Notification:NotificationQueueName"] ?? "notificationqueue", command);
        }
        catch (Exception ex)
        {
            if (command.IsCritical)
            {
                _logger.LogError($"{ex}");
                throw;
            }
            else
                _logger.LogWarning($"{ex}");
        }
    }
}