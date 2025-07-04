using Microsoft.Extensions.Configuration;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace DotNetBrightener.InfisicalVaultClient.Tests;

public class InfisicalSecretClientTest
{
    private readonly ITestOutputHelper _testOutputHelper;

    public InfisicalSecretClientTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void TestInfisicalSecretsProvider_ShouldLoadSecrets()
    {
        // Arrange
        var configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["SecretsManagement:Enabled"]       = "true",
            ["SecretsManagement:ProjectID"]     = "08e9b386-4ba6-4662-ac11-716d9936091d",
            ["SecretsManagement:Environment"]   = "dev",
            ["SecretsManagement:VaultClientID"] = "6187f2c5-28a1-4efc-9629-e75506245b31",
            ["SecretsManagement:VaultClientSecret"] =
                "111dc02562dadc646dafa56e3c09a463bb664742f9e67b6c95734559888893bb",
            // Missing VaultClientID and VaultClientSecret
        });

        // Act
        configurationBuilder.AddInfisicalSecretsProvider();
        var configuration = configurationBuilder.Build();

        // Assert
        var configValues = configuration.AsEnumerable().ToList();

        configValues.ShouldContain(v => v.Key == "ConnectionStrings:DefaultConnectionStringForTesting");
    }
}
