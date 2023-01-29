using Dapr.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace ivp.edm.secrets;

public class SecretsManager
{
    private readonly IConfiguration _configuration;
    private readonly IHostEnvironment _environment;
    private readonly DaprClient _daprClient;
    public SecretsManager(IConfiguration configuration, IHostEnvironment environment)
    {
        _configuration = configuration;
        _environment = environment;
        _daprClient = Register();
    }
    private DaprClient Register()
    {
        var clientBuilder = new DaprClientBuilder();

        if (_environment.IsDevelopment())
            clientBuilder.UseGrpcEndpoint($"http://127.0.0.1:{_configuration["Dapr:SideCarPort"] ?? "50001"}");

        return clientBuilder.Build();
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
        if (string.IsNullOrWhiteSpace(secretKey))
            throw new ArgumentNullException(nameof(secretKey));

        // Get secret from a local secret store
        var secret = await GetDaprSecretAsync(secretStoreName, secretName);

        if (secret.ContainsKey(secretKey))
            return secret[secretKey];
        else
            throw new KeyNotFoundException($"{secretKey} was not found in the {secretName} collection.");
    }

    public async Task<Dictionary<string, string>> GetDaprSecretAsync(string secretStoreName, string secretName)
    {
        if (string.IsNullOrWhiteSpace(secretStoreName))
            throw new ArgumentNullException(nameof(secretStoreName));

        if (string.IsNullOrWhiteSpace(secretName))
            throw new ArgumentNullException(nameof(secretName));

        return await _daprClient.GetSecretAsync(secretStoreName, secretName);
    }
}