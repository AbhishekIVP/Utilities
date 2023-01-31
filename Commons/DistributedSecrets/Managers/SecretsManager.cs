using Dapr.Client;
using ivp.edm.validations;
using Microsoft.Extensions.Configuration;

namespace ivp.edm.secrets;

public class SecretsManager
{
    private readonly IConfiguration _configuration;
    private readonly DaprClient _daprClient;
    public SecretsManager(IConfiguration configuration, DaprClient daprClient)
    {
        _configuration = configuration;
        _daprClient = daprClient;
    }

    public async Task<string> GetDefaultStoreSecretAsync(string secretName)
    {
        string defaultStore = _configuration["Dapr:DefaultSecretStore"] ?? "local";
        return await GetSecretAsync(defaultStore, secretName);
    }

    public async Task<string> GetSecretAsync(string secretStoreName, string secretName)
    {
        // Get secret from a local secret store
        var secret = await GetDaprSecretAsync(secretStoreName, secretName);
        if (secret.ContainsKey(secretName))
            return secret[secretName];
        else
            return string.Join(',', secret);
    }

    public async Task<string> GetSecretAsync(string secretStoreName, string secretName, string secretKey)
    {
        ArgumentGuard.NotNullOrWhiteSpace(secretKey);

        // Get secret from a local secret store
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

        return await _daprClient.GetSecretAsync(secretStoreName, secretName);
    }
}