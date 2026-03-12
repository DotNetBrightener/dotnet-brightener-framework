using Azure.Security.KeyVault.Secrets;
using DotNetBrightener.AzureVaultClient;

// ReSharper disable CheckNamespace
namespace Microsoft.Extensions.Configuration;

public static class ConfigurationBuilderExtensions
{
    public static IConfigurationBuilder
        AddAzureSecretsConfiguration(this IConfigurationBuilder configurationBuilder,
                                     string                     azureKeyVaultUrlConfigName     = "AzureVaultUrl",
                                     string                     vaultSecretKeyIdentifierPrefix = "Secret:") =>
        configurationBuilder.Add(new AzureSecretsConfigurationSource(configurationBuilder.Build(),
                                                                     azureKeyVaultUrlConfigName,
                                                                     vaultSecretKeyIdentifierPrefix));


    internal static async Task<string?> GetSecretValueIfNeeded(this SecretClient secretClient,
                                                               string            secretIdentifierPrefix,
                                                               string            originalValue)
    {
        if (originalValue?.StartsWith(secretIdentifierPrefix, StringComparison.OrdinalIgnoreCase) == true)
        {
            try
            {

                var secretAsync =
                    await secretClient.GetSecretAsync(originalValue.Substring(secretIdentifierPrefix.Length));

                return secretAsync.Value.Value;
            }
            catch (Exception exception)
            {
                throw;
            }
        }

        return originalValue;
    }
}