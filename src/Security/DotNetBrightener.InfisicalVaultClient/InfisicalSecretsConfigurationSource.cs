using Microsoft.Extensions.Configuration;

namespace DotNetBrightener.InfisicalVaultClient;

internal sealed class InfisicalSecretsConfigurationSource : IConfigurationSource
{
    private readonly IConfiguration _originalConfiguration;

    internal InfisicalSecretsConfigurationSource(IConfiguration originalConfiguration)
    {
        _originalConfiguration = originalConfiguration;
    }

    public IConfigurationProvider Build(IConfigurationBuilder builder) => 
        new InfisicalSecretsConfigurationProvider(_originalConfiguration);
}