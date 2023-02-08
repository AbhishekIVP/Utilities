using ivp.edm.secrets;
using ivp.edm.validations;
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
        //TODO: Get redis config from Config Service

        RedisOptions _redisOptions = new RedisOptions();
        configuration.GetSection("Redis").Bind(_redisOptions);

        ConfigurationOptions _options = new ConfigurationOptions();

        ArgumentGuard.NotNullOrEmpty(_redisOptions.Endpoint, "Redis:Endpoint");
        _options.EndPoints.Add(_redisOptions.Endpoint);

        if (string.IsNullOrEmpty(_redisOptions.Password.Value) == false)
            _options.Password = _redisOptions.Password.Value;
        else if (string.IsNullOrEmpty(_redisOptions.Password.ValueFrom) == false)
        {
            SecretsManager _secretsManager = services.ValidatedInstance<SecretsManager>();

            var _secretsServiceType = configuration["SecretsService:Type"];

            ArgumentGuard.NotNull(configuration["SecretsService:Type"], "SecretsService:Type");

            if (_secretsServiceType.ToLower() == "rad")
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
