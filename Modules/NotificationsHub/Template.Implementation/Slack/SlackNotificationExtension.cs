using ivp.edm.secrets;
using ivp.edm.validations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SlackAPI;

namespace ivp.edm.notification.template.implementation;

public static class SlackNotificationExtension
{
    public static IServiceCollection AddSlackNotification(this IServiceCollection services, IConfiguration configuration)
    {
        var _secretsManager = services.ValidatedInstance<SecretsManager>();

        string? token = null;
        var _slackOptions = new SlackOptions();
        configuration.GetSection("Notification:Slack").Bind(_slackOptions);

        if (string.IsNullOrEmpty(_slackOptions.Token.ValueFrom) == false)
            token = _secretsManager.GetDefaultStoreSecretAsync(_slackOptions.Token.ValueFrom).Result;
        else if (string.IsNullOrEmpty(_slackOptions.Token.Value) == false)
            token = _slackOptions.Token.Value;
        SlackClient client = new SlackClient(token);

        ArgumentGuard.NotNull(token);

        services.AddTransient<SlackClient>();
        services.AddTransient<SlackPost>();
        return services;
    }
}

internal class SlackOptions
{
    public PasswordOptions Token { get; set; } = new PasswordOptions();
}