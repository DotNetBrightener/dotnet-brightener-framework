using DotNetBrightener.InfisicalVaultClient;

// ReSharper disable CheckNamespace
namespace Microsoft.Extensions.Configuration;

public static class ConfigurationBuilderExtensions
{
    public static IConfigurationBuilder AddInfisicalSecretsProvider(this IConfigurationBuilder builder) =>
        builder.Add(new InfisicalSecretsConfigurationSource(builder.Build()));
}