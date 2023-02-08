using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ivp.edm.notification.template.implementation;

public static class NotificationExtension
{
    public static IServiceCollection AddNotifications(this IServiceCollection services, IConfiguration configuration)
    {
        var notifiers = configuration.GetSection("Notification").GetChildren();
        foreach (var notifier in notifiers)
        {
            switch (notifier.Key.ToLower())
            {
                case "email":
                    EmailNotificationExtension.AddEmailNotification(services, configuration);
                    break;
                case "slack":
                    SlackNotificationExtension.AddSlackNotification(services, configuration);
                    break;
            }
        }
        services.AddTransient<CommandHandler>();
        return services;
    }
}