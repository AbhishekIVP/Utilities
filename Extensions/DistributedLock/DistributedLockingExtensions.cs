using ivp.edm.validations;
using Medallion.Threading;
using Medallion.Threading.Redis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis;

namespace ivp.edm.distributedlock;

public static class DistributedLockingExtensions
{
    public static IHostBuilder AddDynamicLocking(this IHostBuilder builder)
    {
        return builder.ConfigureServices((context, collection) =>
        {
            collection.AddDynamicLocking(context.Configuration);
        });
    }

    public static IServiceCollection AddDynamicLocking(this IServiceCollection services
                , IConfiguration configuration)
    {
        //TODO: Get Redis Database from Tenant Service

        LockingOptions _lockingOptions = new LockingOptions();
        configuration.GetSection("DistributedLocking").Bind(_lockingOptions);

        switch (_lockingOptions.Type)
        {
            case LockingBackend.Redis:
                IConnectionMultiplexer _redisConnection = services.ValidatedInstance<IConnectionMultiplexer>();
                services.AddSingleton<IDistributedLockProvider>(_ => new RedisDistributedSynchronizationProvider(_redisConnection.GetDatabase(_lockingOptions.RedisDatabase)));
                break;
            default:
                throw new NotImplementedException();
        }
        return services;
    }

    internal class LockingOptions
    {
        public LockingBackend @Type { get; set; } = LockingBackend.Redis;
        public int RedisDatabase { get; set; } = 59;
    }

    internal enum LockingBackend
    {
        Redis
    }
}
