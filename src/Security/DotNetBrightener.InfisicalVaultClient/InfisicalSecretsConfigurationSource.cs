using Microsoft.Extensions.Configuration;

namespace DotNetBrightener.InfisicalVaultClient;

internal sealed class InfisicalSecretsConfigurationSource : IConfigurationSource
{
    private readonly IConfiguration _originalConfiguration;
    private readonly string         _vaultSecretKeyIdentifierPrefix;

    internal InfisicalSecretsConfigurationSource(IConfiguration originalConfiguration,
                                                 string         vaultSecretKeyIdentifierPrefix = "Secret:")
    {
        _originalConfiguration          = originalConfiguration;
        _vaultSecretKeyIdentifierPrefix = vaultSecretKeyIdentifierPrefix;
    }

    public IConfigurationProvider Build(IConfigurationBuilder builder) =>
        new InfisicalSecretsConfigurationProvider(_originalConfiguration,
                                                  _vaultSecretKeyIdentifierPrefix);
}