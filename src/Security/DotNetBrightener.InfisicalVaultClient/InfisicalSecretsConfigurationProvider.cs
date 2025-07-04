using Infisical.Sdk;
using Infisical.Sdk.Model;
using Microsoft.Extensions.Configuration;

namespace DotNetBrightener.InfisicalVaultClient;

internal sealed class InfisicalSecretsConfigurationProvider(IConfiguration originalConfiguration)
    : ConfigurationProvider
{
    public override void Load()
    {
        var secretManagementEnabled = originalConfiguration.GetValue<bool>("SecretsManagement:Enabled");

        if (!secretManagementEnabled)
        {
            return;
        }

        var vaultUrl          = originalConfiguration.GetValue<string>("SecretsManagement:VaultUrl") ?? null;
        var vaultClientId     = originalConfiguration.GetValue<string>("SecretsManagement:VaultClientID");
        var vaultClientSecret = originalConfiguration.GetValue<string>("SecretsManagement:VaultClientSecret");
        var vaultProjectId    = originalConfiguration.GetValue<string>("SecretsManagement:ProjectID");
        var vaultEnvironment  = originalConfiguration.GetValue<string>("SecretsManagement:Environment") ?? "dev";

        if (string.IsNullOrEmpty(vaultClientId) ||
            string.IsNullOrEmpty(vaultClientSecret))
        {
            return;
        }

        var settingsBuilder = new InfisicalSdkSettingsBuilder();

        if (!string.IsNullOrEmpty(vaultUrl))
        {
            settingsBuilder.WithHostUri(vaultUrl);
        }

        var settings = settingsBuilder.Build();

        var infisicalClient = new InfisicalClient(settings);

        var _ = infisicalClient.Auth()
                               .UniversalAuth()
                               .LoginAsync(vaultClientId, vaultClientSecret).Result;


        var options = new ListSecretsOptions
        {
            ProjectId                        = vaultProjectId,
            EnvironmentSlug                  = vaultEnvironment,
            SecretPath                       = "/",
            SetSecretsAsEnvironmentVariables = false,
        };

        var secrets = infisicalClient.Secrets().ListAsync(options).Result;

        foreach (var secretInformation in secrets)
        {
            var confKey = secretInformation.SecretKey.Replace("__", ":");
            Data[confKey] = secretInformation.SecretValue;
        }
    }
}
