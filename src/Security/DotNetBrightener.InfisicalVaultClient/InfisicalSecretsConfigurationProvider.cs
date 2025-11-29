using Infisical.Sdk;
using Infisical.Sdk.Model;
using Microsoft.Extensions.Configuration;
using System.Collections.Concurrent;

namespace DotNetBrightener.InfisicalVaultClient;

internal sealed class InfisicalSecretsConfigurationProvider(IConfiguration originalConfiguration,
                                                            string         vaultSecretKeyIdentifierPrefix = "Secret:")
    : ConfigurationProvider
{
    public override void Load()
    {
        var secretManagementEnabled = originalConfiguration.GetValue<bool>("Infisical:Enabled");

        if (!secretManagementEnabled)
        {
            return;
        }

        var vaultUrl          = originalConfiguration.GetValue<string>("Infisical:VaultUrl");
        var vaultClientId     = originalConfiguration.GetValue<string>("Infisical:VaultClientID");
        var vaultClientSecret = originalConfiguration.GetValue<string>("Infisical:VaultClientSecret");
        var vaultProjectId    = originalConfiguration.GetValue<string>("Infisical:ProjectID");
        var vaultEnvironment  = originalConfiguration.GetValue<string>("Infisical:Environment") ?? "dev";

        if (string.IsNullOrEmpty(vaultClientId) ||
            string.IsNullOrEmpty(vaultClientSecret))
        {
            return;
        }

        var settingsBuilder = new InfisicalSdkSettingsBuilder();

        if (!string.IsNullOrWhiteSpace(vaultUrl))
        {
            settingsBuilder.WithHostUri(vaultUrl);
        }

        var settings = settingsBuilder.Build();

        var infisicalClient = new InfisicalClient(settings);

        var _ = infisicalClient.Auth()
                               .UniversalAuth()
                               .LoginAsync(vaultClientId, vaultClientSecret)
                               .Result;

        var options = new ListSecretsOptions
        {
            ProjectId                        = vaultProjectId,
            EnvironmentSlug                  = vaultEnvironment,
            SecretPath                       = "/",
            SetSecretsAsEnvironmentVariables = false,
        };

        var secrets = infisicalClient.Secrets().ListAsync(options).Result;


        var configuration = originalConfiguration.AsEnumerable()
                                                 .Where(c => c.Value?.StartsWith(vaultSecretKeyIdentifierPrefix) ==
                                                             true)
                                                 .ToArray();

        foreach (var secretInformation in secrets)
        {
            var confKey = secretInformation.SecretKey.Replace("__", ":");
            Data[confKey] = secretInformation.SecretValue;
        }

        var concurrentData = new ConcurrentDictionary<string, string>();

        configuration.ParallelForEachAsync(async keyPair =>
                      {
                          var secretKey = keyPair.Value.Substring(vaultSecretKeyIdentifierPrefix.Length);
                          var secretInformation =
                              secrets.FirstOrDefault(s => s.SecretKey == secretKey);
                          if (secretInformation is not null)
                          {
                              concurrentData.TryAdd(keyPair.Key, secretInformation.SecretValue);
                          }
                      })
                     .Wait();

        foreach (var (key, value) in concurrentData)
        {
            Data[key] = value;
        }
    }
}
