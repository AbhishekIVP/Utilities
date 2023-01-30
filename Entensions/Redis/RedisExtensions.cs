using ivp.edm.secrets;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis;

namespace ivp.edm.redis;
public static class RedisExtensions
{
    public static IHostBuilder AddRedisClient(this IHostBuilder builder)
    {
        return builder.ConfigureServices((context, collection) =>
        {
            collection.AddRedisClient(context.Configuration);
        });
    }

    public static IServiceCollection AddRedisClient(this IServiceCollection services, IConfiguration configuration)
    {
        RedisOptions _redisOptions = new RedisOptions();
        configuration.GetSection("Redis").Bind(_redisOptions);

        ConfigurationOptions _options = new ConfigurationOptions();

        if (string.IsNullOrEmpty(_redisOptions.Endpoint))
            throw new ArgumentNullException("Redis:Endpoint");
        else
            _options.EndPoints.Add(_redisOptions.Endpoint);

        if (string.IsNullOrEmpty(_redisOptions.Password.Value) == false)
            _options.Password = _redisOptions.Password.Value;

        else if (string.IsNullOrEmpty(_redisOptions.Password.ValueFrom) == false)
        {
            SecretsManager? _secretsManager;
            using (var _serviceProvider = services.BuildServiceProvider())
            {
                _secretsManager = _serviceProvider?.GetService<SecretsManager>();
                if (_secretsManager == null)
                    throw new ArgumentNullException($"{nameof(SecretsManager)} is not added to the service collection.");
            }
            var _secretsServiceType = configuration["SecretsService:Type"];
            if (_secretsServiceType == null)
                throw new ArgumentNullException("SecretsService:Type");
            else if (_secretsServiceType.ToLower() == "rad")
                _options.Password = _secretsManager.GetDefaultStoreSecretAsync(_redisOptions.Password.ValueFrom).Result;
            else
                throw new NotImplementedException($"Secrets Service {_secretsServiceType} type is not implemented.");
        }
        else
            throw new ArgumentNullException("Redis:Password");

        services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(_options));
        return services;
    }

    internal class RedisOptions
    {
        public string Endpoint { get; set; } = string.Empty;
        public RedisPassword @Password { get; set; } = new RedisPassword();
    }

    internal class RedisPassword
    {
        public string Value { get; set; } = string.Empty;
        public string ValueFrom { get; set; } = string.Empty;
    }
}
