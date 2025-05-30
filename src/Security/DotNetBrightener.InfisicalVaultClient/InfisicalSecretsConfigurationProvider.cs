using Infisical.Sdk;
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

        ClientSettings settings = new ClientSettings
        {

            Auth = new AuthenticationOptions
            {
                UniversalAuth = new UniversalAuthMethod
                {
                    ClientId     = vaultClientId,
                    ClientSecret = vaultClientSecret
                }
            }
        };

        if (!string.IsNullOrEmpty(vaultUrl))
        {
            settings.SiteUrl = vaultUrl;
        }


        var infisicalClient = new InfisicalClient(settings);

        var options = new ListSecretsOptions
        {
            ProjectId          = vaultProjectId,
            Environment        = vaultEnvironment,
            Path               = "",
            AttachToProcessEnv = false,
        };

        var secrets = infisicalClient.ListSecrets(options);

        foreach (var secretInformation in secrets)
        {
            var confKey = secretInformation.SecretKey.Replace("__", ":");
            Data[confKey] = secretInformation.SecretValue;
        }
    }
}