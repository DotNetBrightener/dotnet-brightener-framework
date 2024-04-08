using System.Collections.Concurrent;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Configuration;

namespace DotNetBrightener.AzureVaultClient;

internal class AzureSecretsConfigurationProvider : ConfigurationProvider
{
    private readonly IConfiguration _originalConfiguration;
    private readonly string         _vaultSecretKeyIdentifierPrefix;
    private readonly string         _azureKeyVaultUrlConfigName;
    private readonly SecretClient?  _secretClient = null;

    public AzureSecretsConfigurationProvider(IConfiguration originalConfiguration,
                                             string         vaultSecretKeyIdentifierPrefix = "Secret:",
                                             string         azureKeyVaultUrlConfigName     = "AzureVaultUrl")
    {
        _originalConfiguration          = originalConfiguration;
        _vaultSecretKeyIdentifierPrefix = vaultSecretKeyIdentifierPrefix;
        _azureKeyVaultUrlConfigName     = azureKeyVaultUrlConfigName;

        var vaultUrl = _originalConfiguration.GetValue<string>(_azureKeyVaultUrlConfigName);

        if (vaultUrl is not null)
        {
            _secretClient = new SecretClient(vaultUri: new Uri(vaultUrl), credential: new DefaultAzureCredential());
        }
    }

    public override void Load()
    {
        if (_secretClient is null)
        {
            return;
        }

        var configuration = _originalConfiguration.AsEnumerable()
                                                  .Where(_ => _.Value?.StartsWith(_vaultSecretKeyIdentifierPrefix) ==
                                                              true)
                                                  .ToArray();
        var concurrentData = new ConcurrentDictionary<string, string>();
        Parallel.ForEachAsync(configuration,
                              async (keyPair, obj) =>
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