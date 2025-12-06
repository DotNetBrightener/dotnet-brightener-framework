using System.Collections.Concurrent;
using Azure.Core;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Configuration;

namespace DotNetBrightener.AzureVaultClient;

internal class AzureSecretsConfigurationProvider : ConfigurationProvider
{
    private readonly IConfiguration _originalConfiguration;
    private readonly string         _vaultSecretKeyIdentifierPrefix;
    private readonly SecretClient   _secretClient;

    public AzureSecretsConfigurationProvider(IConfiguration originalConfiguration,
                                             string         vaultSecretKeyIdentifierPrefix = "Secret:",
                                             string         azureKeyVaultUrlConfigName     = "AzureVaultUrl")
    {
        _originalConfiguration          = originalConfiguration;
        _vaultSecretKeyIdentifierPrefix = vaultSecretKeyIdentifierPrefix;

        var vaultUrl = _originalConfiguration.GetValue<string>(azureKeyVaultUrlConfigName);

        var appRegistrationClientId = _originalConfiguration.GetValue<string?>("AzureAd:ClientId");
        var appRegistrationTenantId = _originalConfiguration.GetValue<string?>("AzureAd:TenantId");
        var appRegistrationClientSecret = _originalConfiguration.GetValue<string?>("AzureAd:ClientSecret");

        TokenCredential credential = new DefaultAzureCredential(); 

        if (!string.IsNullOrWhiteSpace(appRegistrationClientId) && 
            !string.IsNullOrWhiteSpace(appRegistrationTenantId) && 
            !string.IsNullOrWhiteSpace(appRegistrationClientSecret))
        {
            credential = new ClientSecretCredential(
                tenantId: appRegistrationTenantId,
                clientId: appRegistrationClientId,
                clientSecret: appRegistrationClientSecret
            );
        }

        _secretClient = vaultUrl is not null
                            ? new SecretClient(vaultUri: new Uri(vaultUrl), credential: credential)
                            : throw new
                                  InvalidOperationException("Cannot obtain Azure Vault URL. Either remove this Configuration Source, or provide the valid Azure Vault URL");
    }

    public override void Load()
    {
        if (_secretClient is null) // safety check, it should never happen
            return;

        var configuration = _originalConfiguration.AsEnumerable()
                                                  .Where(c => c.Value?.StartsWith(_vaultSecretKeyIdentifierPrefix) ==
                                                              true)
                                                  .ToArray();

        var secrets = GetSecretsWithThrottlingAsync(_secretClient).Result;

        var secretBasedConfigEntries = new ConcurrentDictionary<string, string>();

        configuration.ParallelForEachAsync(async (keyPair) =>
                      {
                          var secretKey = keyPair.Value!.Substring(_vaultSecretKeyIdentifierPrefix.Length);

                          var secretValue =
                              secrets.GetValueOrDefault(secretKey);

                          if (!string.IsNullOrWhiteSpace(secretValue))
                          {
                              secretBasedConfigEntries.TryAdd(keyPair.Key, secretValue);
                              secrets.Remove(secretKey);
                          }
                      })
                     .Wait();

        foreach (var (key, value) in secretBasedConfigEntries)
        {
            Data[key] = value;
        }

        foreach (var (key, value) in secrets)
        {
            Data[key] = value;
        }
    }

    private static async Task<Dictionary<string, string>> GetSecretsWithThrottlingAsync(SecretClient client,
                                                                                        int batchSize = 50)
    {
        var secrets     = new Dictionary<string, string>();
        var secretNames = new List<string>();

        // Get all names first
        await foreach (var props in client.GetPropertiesOfSecretsAsync())
        {
            if (props.Enabled == true)
                secretNames.Add(props.Name);
        }

        // Fetch in small batches with delay
        for (int i = 0; i < secretNames.Count; i += batchSize)
        {
            var batch   = secretNames.Skip(i).Take(batchSize);
            var tasks   = batch.Select(name => client.GetSecretAsync(name));
            var results = await Task.WhenAll(tasks);

            foreach (var result in results)
            {
                secrets[result.Value.Name.Replace("--", ":")] = result.Value.Value;
            }

            // Add small delay between batches if needed
            if (i + batchSize < secretNames.Count)
                await Task.Delay(100);
        }

        return secrets;
    }
}