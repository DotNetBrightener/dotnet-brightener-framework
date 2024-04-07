using Microsoft.Extensions.Configuration;

namespace DotNetBrightener.AzureVaultClient;

internal sealed class AzureSecretsConfigurationSource : IConfigurationSource
{
    private readonly IConfiguration _originalConfiguration;
    private readonly string         _vaultSecretKeyIdentifierPrefix;
    private readonly string         _azureKeyVaultUrlConfigName;

    public AzureSecretsConfigurationSource(IConfiguration originalConfiguration,
                                           string         azureKeyVaultUrlConfigName,
                                           string         vaultSecretKeyIdentifierPrefix = "Secret:")
    {
        _originalConfiguration          = originalConfiguration;
        _vaultSecretKeyIdentifierPrefix = vaultSecretKeyIdentifierPrefix;
        _azureKeyVaultUrlConfigName     = azureKeyVaultUrlConfigName;
    }

    public IConfigurationProvider Build(IConfigurationBuilder builder) =>
        new AzureSecretsConfigurationProvider(_originalConfiguration,
                                              _vaultSecretKeyIdentifierPrefix,
                                              _azureKeyVaultUrlConfigName);
}