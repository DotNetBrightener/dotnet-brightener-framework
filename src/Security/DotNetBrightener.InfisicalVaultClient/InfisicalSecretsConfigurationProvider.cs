using Microsoft.Extensions.Configuration;

namespace DotNetBrightener.InfisicalVaultClient;

internal sealed class InfisicalSecretsConfigurationProvider : ConfigurationProvider
{
    private readonly IConfiguration _originalConfiguration;

    public InfisicalSecretsConfigurationProvider(IConfiguration originalConfiguration)
    {
        _originalConfiguration = originalConfiguration;
    }

    public override void Load()
    {
        var secretManagementEnabled = _originalConfiguration.GetValue<bool>("SecretsManagement:Enabled");

        if (!secretManagementEnabled)
        {
            return;
        }

        var vaultUrl         = _originalConfiguration.GetValue<string>("SecretsManagement:VaultUrl");
        var vaultAccessToken = _originalConfiguration.GetValue<string>("SecretsManagement:VaultAccessKey");
        var vaultProjectId   = _originalConfiguration.GetValue<string>("SecretsManagement:ProjectID");
        var vaultEnvironment = _originalConfiguration.GetValue<string>("SecretsManagement:Environment") ?? "dev";

        if (string.IsNullOrEmpty(vaultUrl) ||
            string.IsNullOrEmpty(vaultAccessToken) ||
            string.IsNullOrEmpty(vaultProjectId))
        {
            return;
        }

        var secretClient = new InfisicalSecretClient(vaultUrl, vaultAccessToken);
        secretClient.ChangeEnvironment(vaultEnvironment);

        var secrets = secretClient.RetrieveSecrets(vaultProjectId)
                                  .Result;

        foreach (var secretInformation in secrets)
        {
            var confKey = secretInformation.SecretKey.Replace("__", ":");
            Data[confKey] = secretInformation.SecretValue;
        }
    }
}