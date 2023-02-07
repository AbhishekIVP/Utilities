using FluentEmail.MailKitSmtp;
using ivp.edm.secrets;
using ivp.edm.validations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ivp.edm.notification.template.implementation;

public static class EmailNotificationExtension
{
    public static IServiceCollection AddEmailNotification(this IServiceCollection services, IConfiguration configuration)
    {
        var _secretsManager = services.ValidatedInstance<SecretsManager>();

        var _notificationOptions = new EmailNotificationOptions();
        configuration.GetSection("Notification:Email").Bind(_notificationOptions);

        if (string.IsNullOrEmpty(_notificationOptions.Password.ValueFrom) == false)
            ((SmtpClientOptions)_notificationOptions).Password = _secretsManager.GetDefaultStoreSecretAsync(_notificationOptions.Password.ValueFrom).Result;
        else if (string.IsNullOrEmpty(_notificationOptions.Password.Value) == false)
            ((SmtpClientOptions)_notificationOptions).Password = _notificationOptions.Password.Value;

        services.AddFluentEmail(_notificationOptions.FromEmail)
                .AddRazorRenderer()
                .AddMailKitSender(_notificationOptions);

        services.AddTransient<Email>();
        return services;
    }
}
public class EmailNotificationOptions : SmtpClientOptions
{
    public new PasswordOptions Password { get; set; } = new PasswordOptions();
    public string FromEmail { get; set; } = "notifications@ivp.in";
}

public class PasswordOptions
{
    public string Value { get; set; } = string.Empty;
    public string ValueFrom { get; set; } = string.Empty;
}