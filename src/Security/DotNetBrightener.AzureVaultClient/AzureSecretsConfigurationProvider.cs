using System.Collections.Concurrent;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Configuration;

namespace DotNetBrightener.AzureVaultClient;

internal class AzureSecretsConfigurationProvider : ConfigurationProvider
{
    private readonly IConfiguration _originalConfiguration;
    private readonly string         _vaultSecretKeyIdentifierPrefix;
    private readonly SecretClient?  _secretClient = null;

    public AzureSecretsConfigurationProvider(IConfiguration originalConfiguration,
                                             string         vaultSecretKeyIdentifierPrefix = "Secret:",
                                             string         azureKeyVaultUrlConfigName     = "AzureVaultUrl")
    {
        _originalConfiguration          = originalConfiguration;
        _vaultSecretKeyIdentifierPrefix = vaultSecretKeyIdentifierPrefix;

        var vaultUrl = _originalConfiguration.GetValue<string>(azureKeyVaultUrlConfigName);

        if (vaultUrl is null) 
            throw new InvalidOperationException("Cannot obtain Azure Vault URL. Either remove this Configuration Source, or provide the valid Azure Vault URL");

        _secretClient = new SecretClient(vaultUri: new Uri(vaultUrl), credential: new DefaultAzureCredential());
    }

    public override void Load()
    {
        if (_secretClient is null)
        {
            return;
        }

        var configuration = _originalConfiguration.AsEnumerable()
                                                  .Where(c => c.Value?.StartsWith(_vaultSecretKeyIdentifierPrefix) ==
                                                              true)
                                                  .ToArray();

        var concurrentData = new ConcurrentDictionary<string, string>();

        configuration.ParallelForEachAsync(async (keyPair) =>
                      {
                          var secretValue =
                              await _secretClient
                                 .GetSecretValueIfNeeded(_vaultSecretKeyIdentifierPrefix,
                                                         keyPair.Value);

                          concurrentData.TryAdd(keyPair.Key, secretValue);
                      })
                     .Wait();

        foreach (var (key, value) in concurrentData)
        {
            Data[key] = value;
        }
    }
}