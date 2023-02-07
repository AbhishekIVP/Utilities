using Dapr.Client;
using ivp.edm.validations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ivp.edm.secrets;

public class SecretsManager
{
    private readonly ILogger<SecretsManager> _logger;
    private readonly IConfiguration _configuration;
    private readonly DaprClient _daprClient;

    public SecretsManager(IConfiguration configuration, ILogger<SecretsManager> logger, DaprClient daprClient)
    {
        _logger = logger;
        _configuration = configuration;
        _daprClient = daprClient;
    }

    public async Task<string> GetDefaultStoreSecretAsync(string secretName)
    {
        string defaultStore = _configuration["Application:DefaultSecretStore"] ?? "local";

        _logger.LogInformation($"MethodName GetDefaultStoreSecretAsync / SecretStoreName {defaultStore}");
        _logger.LogDebug($"secretName {secretName}");

        return await GetSecretAsync(defaultStore, secretName);
    }

    public async Task<string> GetSecretAsync(string secretStoreName, string secretName)
    {
        _logger.LogInformation($"MethodName GetSecretAsync / SecretStoreName {secretStoreName}");
        _logger.LogDebug($"secretName {secretName}");

        var secret = await GetDaprSecretAsync(secretStoreName, secretName);
        if (secret.ContainsKey(secretName))
            return secret[secretName];
        else
            return string.Join(',', secret);
    }

    public async Task<string> GetSecretAsync(string secretStoreName, string secretName, string secretKey)
    {
        ArgumentGuard.NotNullOrWhiteSpace(secretKey);

        _logger.LogInformation($"MethodName GetSecretAsync / SecretStoreName {secretStoreName}");
        _logger.LogDebug($"secretName {secretName} / secretKey {secretKey}");

        var secret = await GetDaprSecretAsync(secretStoreName, secretName);

        if (secret.ContainsKey(secretKey))
            return secret[secretKey];
        else
            throw new KeyNotFoundException($"{secretKey} was not found in the {secretName} collection.");
    }

    public async Task<Dictionary<string, string>> GetDaprSecretAsync(string secretStoreName, string secretName)
    {
        ArgumentGuard.NotNullOrWhiteSpace(secretStoreName);
        ArgumentGuard.NotNullOrWhiteSpace(secretName);

        secretStoreName = GetMultiTenantSecretStoreName(secretStoreName);

        _logger.LogInformation($"MethodName GetDaprSecretAsync / SecretStoreName {secretStoreName}");
        _logger.LogDebug($"secretName {secretName}");

        return await _daprClient.GetSecretAsync(secretStoreName, secretName);
    }

    private string GetMultiTenantSecretStoreName(string secretStoreName)
    {
        bool _isMultiTenant = Convert.ToBoolean(_configuration["Application:IsMultiTenant"]);
        if (_isMultiTenant)
        {
            string clientName = "client2";
            _logger.LogDebug($"Client Name {clientName}");
            //TODO:ATTACH Current Request CLIENT ID IF MULTI TENANT from Tenancy Provider and build pubsub name
            return $"{secretStoreName}-{clientName}";
        }
        else
            return secretStoreName;
    }
}