using Dapr.Client;
using ivp.edm.apm;
using ivp.edm.distributedlock;
using ivp.edm.pubsub;
using ivp.edm.redis;
using ivp.edm.secrets;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace ivp.edm;

public static class MasterExtensions
{
    public static WebApplicationBuilder AddExtensions(this WebApplicationBuilder builder)
    {
        builder.AddExtensions(Enable.ALL);
        return builder;
    }

    public static WebApplicationBuilder AddExtensions(this WebApplicationBuilder builder, Enable extensions)
    {
        if (extensions.HasFlag(Enable.LOG) || extensions.HasFlag(Enable.ALL))
            builder.Logging.AddLogging(builder.Configuration, builder.Environment);

        if (extensions.HasFlag(Enable.LOCKING) ||
                extensions.HasFlag(Enable.DAPR) ||
                    extensions.HasFlag(Enable.REDIS) ||
                        extensions.HasFlag(Enable.SECRETS) ||
                            extensions.HasFlag(Enable.PUB_SUB) ||
                                extensions.HasFlag(Enable.ALL))
            builder.Services.AddSingleton<DaprClient>(new DaprClientBuilder().Build());

        if (extensions.HasFlag(Enable.LOCKING) || extensions.HasFlag(Enable.REDIS) || extensions.HasFlag(Enable.SECRETS) || extensions.HasFlag(Enable.ALL))
            builder.Services.AddScoped<SecretsManager>();

        if (extensions.HasFlag(Enable.LOCKING) || extensions.HasFlag(Enable.REDIS) || extensions.HasFlag(Enable.ALL))
            builder.Services.AddRedisClient(builder.Configuration);

        if (extensions.HasFlag(Enable.LOCKING) || extensions.HasFlag(Enable.ALL))
            builder.Services.AddDynamicLocking(builder.Configuration);

        if (extensions.HasFlag(Enable.APM) || extensions.HasFlag(Enable.ALL))
            builder.Services.AddMonitoring(builder.Configuration, builder.Environment);

        if (extensions.HasFlag(Enable.PUB_SUB) || extensions.HasFlag(Enable.ALL))
            builder.Services.InitializePubSub(builder.Configuration);

        return builder;
    }
}
public enum Enable
{
    ALL = 1,
    DAPR = 2,
    SECRETS = 4,
    REDIS = 8,
    LOCKING = 16,
    APM = 32,
    LOG = 64,
    PUB_SUB = 128
}