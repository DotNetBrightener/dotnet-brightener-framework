using Microsoft.Extensions.Configuration;

namespace DotNetBrightener.AzureVaultClient;

internal sealed class AzureSecretsConfigurationSource(
    IConfiguration originalConfiguration,
    string         azureKeyVaultUrlConfigName,
    string         vaultSecretKeyIdentifierPrefix = "Secret:")
    : IConfigurationSource
{
    public IConfigurationProvider Build(IConfigurationBuilder builder) =>
        new AzureSecretsConfigurationProvider(originalConfiguration,
                                              vaultSecretKeyIdentifierPrefix,
                                              azureKeyVaultUrlConfigName);
}