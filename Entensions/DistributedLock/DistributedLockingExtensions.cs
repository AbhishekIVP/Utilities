using ivp.edm.secrets;
using Medallion.Threading;
using Medallion.Threading.Redis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis;

namespace ivp.edm.distributedlock;

public static class DistributedLockingExtensions
{
    public static IHostBuilder AddDynamicRedisLocking(this IHostBuilder builder, Action<IConnectionMultiplexer>? connectionMultiplexer = null)
    {
        return builder.ConfigureServices((context, collection) =>
        {
            collection.AddDynamicRedisLocking(context.Configuration, context.HostingEnvironment, connectionMultiplexer);
        });
    }

    public static IServiceCollection AddDynamicRedisLocking(this IServiceCollection services
            , IConfiguration configuration
            , IHostEnvironment environment
            , Action<IConnectionMultiplexer>? connectionMultiplexer = null)
    {
        ConfigurationOptions _options = new ConfigurationOptions();

        if (string.IsNullOrEmpty(configuration["DistributedLocking:Redis:Password:Value"]) == false)
            _options.Password = configuration["DistributedLocking:Redis:Password:Value"];

        else if (string.IsNullOrEmpty(configuration["DistributedLocking:Redis:Password:ValueFrom"]) == false)
        {
            var _passwordKey = configuration["DistributedLocking:Redis:Password:ValueFrom"];
            if (_passwordKey == null)
                throw new ArgumentNullException("DistributedLocking:Redis:Password:ValueFrom");

            var _serviceProvider = services.BuildServiceProvider();
            SecretsManager? _secretsManager = _serviceProvider?.GetService<SecretsManager>();
            if (_secretsManager == null)
                throw new ArgumentNullException($"{nameof(SecretsManager)} is not added to the service collection.");

            var _secretsServiceType = configuration["SecretsService:Type"];
            if (_secretsServiceType?.ToLower() == "rad")
                _options.Password = _secretsManager?.GetDefaultStoreSecretAsync(_passwordKey).Result;
            else if (_secretsServiceType == null)
                throw new ArgumentNullException("SecretsService:Type");
            else
                throw new NotImplementedException($"Secrets Service {_secretsServiceType} type is not implemented.");
        }

        _options.DefaultDatabase = Convert.ToInt32(configuration["DistributedLocking:Redis:Database"]);

        if (string.IsNullOrEmpty(configuration["DistributedLocking:Redis:Endpoint"]))
            throw new ArgumentNullException("DistributedLocking:Redis:Endpoint");

        _options.EndPoints.Add(configuration["DistributedLocking:Redis:Endpoint"]);
        var connection = ConnectionMultiplexer.Connect(_options); // uses StackExchange.Redis

        services.AddSingleton<IDistributedLockProvider>(_ => new RedisDistributedSynchronizationProvider(connection.GetDatabase()));

        connectionMultiplexer?.Invoke(connection);

        return services;
    }
}
