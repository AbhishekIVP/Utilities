using Microsoft.AspNetCore.Mvc;

namespace ivp.edm.secrets;

[ApiController]
[Route("[controller]")]
public class SecretsController : ControllerBase
{
    private readonly SecretsManager _secretManager;

    public SecretsController(SecretsManager secretManager)
    {
        _secretManager = secretManager;
    }

    [HttpGet]
    [Route("/secret/{secretStoreName=local}/{secretName=default}")]
    public async Task<string> GetSecret(string secretStoreName, string secretName)
    {
        return await _secretManager.GetSecretAsync(secretStoreName, secretName);
    }

    [HttpGet]
    [Route("/secret/{secretName}")]
    public async Task<string> GetDefaultStoreSecret(string secretName)
    {
        return await _secretManager.GetDefaultStoreSecretAsync(secretName);
    }

    [HttpGet]
    [Route("/secret/{secretStoreName}/{secretName}/{secretKey}")]
    public async Task<string> GetSecret(string secretStoreName, string secretName, string secretKey)
    {
        return await _secretManager.GetSecretAsync(secretStoreName, secretName, secretKey);
    }

    [HttpGet]
    [Route("/secrets/{secretStoreName}/{secretName}")]
    public async Task<Dictionary<string, string>> GetDaprSecretAsync(string secretStoreName, string secretName)
    {
        return await _secretManager.GetDaprSecretAsync(secretStoreName, secretName);
    }
}