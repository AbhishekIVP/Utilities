using Dapr.Client;
using ivp.edm.apm;
using ivp.edm.distributedlock;
using ivp.edm.redis;
using ivp.edm.secrets;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace ivp.edm;

public static class MasterExtensions
{
    public static WebApplicationBuilder AddEverything(this WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<DaprClient>(new DaprClientBuilder().Build());
        builder.Services.AddScoped<SecretsManager>();

        builder.Services.AddRedisClient(builder.Configuration);
        builder.Services.AddDynamicLocking(builder.Configuration);

        builder.Services.AddMonitoring(builder.Configuration, builder.Environment);

        builder.Logging.AddLogging(builder.Configuration, builder.Environment);

        return builder;
    }

    public static WebApplicationBuilder AddExtensions(this WebApplicationBuilder builder, Enable extensions)
    {
        if (extensions.HasFlag(Enable.LOG))
            builder.Logging.AddLogging(builder.Configuration, builder.Environment);

        if (extensions.HasFlag(Enable.LOCKING | Enable.DAPR | Enable.REDIS | Enable.SECRETS))
            builder.Services.AddSingleton<DaprClient>(new DaprClientBuilder().Build());

        if (extensions.HasFlag(Enable.LOCKING | Enable.REDIS | Enable.SECRETS))
            builder.Services.AddScoped<SecretsManager>();

        if (extensions.HasFlag(Enable.LOCKING | Enable.REDIS))
            builder.Services.AddRedisClient(builder.Configuration);

        if (extensions.HasFlag(Enable.LOCKING))
            builder.Services.AddDynamicLocking(builder.Configuration);

        if (extensions.HasFlag(Enable.APM))
            builder.Services.AddMonitoring(builder.Configuration, builder.Environment);

        return builder;
    }
}
public enum Enable
{
    DAPR = 0,
    SECRETS = 1,
    REDIS = 2,
    LOCKING = 4,
    APM = 8,
    LOG = 16
}