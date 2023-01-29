using Dapr.Secrets;
using Medallion.Threading;
using Medallion.Threading.Redis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis;

namespace ivp.edm.distributedlock;

public static class DistributedLockingExtensions
{
    // public static IHostBuilder AddDynamicRedisLocking(this IHostBuilder builder)
    // {
    //     return builder.ConfigureServices((context, collection) =>
    //     {
    //         //TODO: Should be a better way
    //         string _uri = context.Configuration["SecretsService:Endpoint"];
    //         if (string.IsNullOrEmpty(_uri))
    //             throw new ArgumentNullException("SecretsService:Endpoint");

    //         collection.AddHttpClient("secrets", _ =>
    //         {
    //             _.BaseAddress = new Uri(_uri);
    //         });
    //         collection.AddDynamicRedisLocking(context.Configuration);
    //     });
    // }

    // public static IServiceCollection AddDynamicRedisLocking(this IServiceCollection services, IConfiguration configuration)
    // {
    //     ConfigurationOptions _options = new ConfigurationOptions();

    //     if (string.IsNullOrEmpty(configuration["DistributedLocking:Redis:Password:Value"]) == false)
    //         _options.Password = configuration["DistributedLocking:Redis:Password:Value"];

    //     else if (string.IsNullOrEmpty(configuration["DistributedLocking:Redis:Password:ValueFrom"]) == false)
    //     {
    //         var _passwordKey = configuration["DistributedLocking:Redis:Password:ValueFrom"];
    //         if (_passwordKey == null)
    //             throw new ArgumentNullException("DistributedLocking:Redis:Password:ValueFrom");

    //         var _serviceProvider = services.BuildServiceProvider();
    //         var _httpClientFactory = _serviceProvider.GetService<IHttpClientFactory>();

    //         if (_httpClientFactory == null)
    //             throw new ArgumentNullException($"HttpClient is not added to the Service Collection{Environment.NewLine}Either add it explicitly or Use builder.Host.AddDynamicLocking.");

    //         using (var _httpClient = _httpClientFactory.CreateClient("secrets"))
    //         {
    //             var _secretsServiceType = configuration["SecretsService:Type"];
    //             if (_secretsServiceType?.ToLower() == "rad")
    //                 //TODO: Should be a better way than calling .Result on async
    //                 _options.Password = _httpClient.GetStringAsync($"secret/{_passwordKey}").Result;
    //             else if (_secretsServiceType == null)
    //                 throw new ArgumentNullException("SecretsService:Type");
    //             else
    //                 throw new NotImplementedException($"Secrets Service {_secretsServiceType} type is not implemented.");
    //         }
    //     }
    //     _options.DefaultDatabase = Convert.ToInt32(configuration["DistributedLocking:Redis:Database"]);

    //     if (string.IsNullOrEmpty(configuration["DistributedLocking:Redis:Endpoint"]))
    //         throw new ArgumentNullException("DistributedLocking:Redis:Endpoint");

    //     _options.EndPoints.Add(configuration["DistributedLocking:Redis:Endpoint"]);
    //     var connection = ConnectionMultiplexer.Connect(_options); // uses StackExchange.Redis

    //     services.AddSingleton<IDistributedLockProvider>(_ => new RedisDistributedSynchronizationProvider(connection.GetDatabase()));

    //     return services;
    // }

    public static IHostBuilder AddDynamicRedisLocking(this IHostBuilder builder)
    {
        return builder.ConfigureServices((context, collection) =>
        {
            collection.AddDynamicRedisLocking(context.Configuration, context.HostingEnvironment);
        });
    }

    public static IServiceCollection AddDynamicRedisLocking(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        ConfigurationOptions _options = new ConfigurationOptions();

        if (string.IsNullOrEmpty(configuration["DistributedLocking:Redis:Password:Value"]) == false)
            _options.Password = configuration["DistributedLocking:Redis:Password:Value"];

        else if (string.IsNullOrEmpty(configuration["DistributedLocking:Redis:Password:ValueFrom"]) == false)
        {
            var _passwordKey = configuration["DistributedLocking:Redis:Password:ValueFrom"];
            if (_passwordKey == null)
                throw new ArgumentNullException("DistributedLocking:Redis:Password:ValueFrom");


            SecretsManager _secretsManager = new SecretsManager(configuration, environment);

            var _secretsServiceType = configuration["SecretsService:Type"];
            if (_secretsServiceType?.ToLower() == "rad")
                //TODO: Should be a better way than calling .Result on async
                _options.Password = _secretsManager.GetDefaultStoreSecretAsync(_passwordKey).Result;
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

        return services;
    }
}
