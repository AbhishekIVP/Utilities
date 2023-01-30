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

        LockingOptions _lockingOptions = new LockingOptions();
        configuration.GetSection("DistributedLocking").Bind(_lockingOptions);

        switch (_lockingOptions.Type)
        {
            case LockingBackend.Redis:
                IConnectionMultiplexer? _redisConnection;
                using (var _serviceProvider = services.BuildServiceProvider())
                    _redisConnection = _serviceProvider.GetService<IConnectionMultiplexer>();
                if (_redisConnection != null)
                    services.AddSingleton<IDistributedLockProvider>(_ => new RedisDistributedSynchronizationProvider(_redisConnection.GetDatabase(_lockingOptions.RedisDatabase)));
                else
                    throw new ArgumentNullException("IConnectionMultiplexer is not added to the list of services.");
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
