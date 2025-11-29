using Microsoft.Extensions.Configuration;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace DotNetBrightener.InfisicalVaultClient.Tests;

public class InfisicalSecretClientTest(ITestOutputHelper testOutputHelper)
{
    private readonly ITestOutputHelper _testOutputHelper = testOutputHelper;

    [Fact]
    public void TestInfisicalSecretsProvider_ShouldLoadSecrets()
    {
        // Arrange
        var configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Infisical:Enabled"]       = "true",
            ["Infisical:ProjectID"]     = "6aea8d7a-a4e2-41df-96f7-a5fa20d9853f",
            ["Infisical:Environment"]   = "dev",
            ["Infisical:VaultClientID"] = "d90475b4-144d-444e-9fde-15461c21e4b1",
            ["Infisical:VaultClientSecret"] =
                "d41ba8e700cea53dc4b78d609fd24b01c4cafa426dc7e0d1b23fe003d81b1023",
            ["ParentKey:ChildKey"] = "Secret:ConnectionStrings_TestValueSecret"
        });

        // Act
        configurationBuilder.AddInfisicalSecretsProvider();
        var configuration = configurationBuilder.Build();

        // Assert
        var configValues = configuration.AsEnumerable().ToList();

        configValues.ShouldContain(v => v.Key == "ConnectionStrings:DefaultConnectionStringForTesting");

        configValues.ShouldContain(v => v.Key == "ConnectionStrings:DefaultConnectionStringForTesting" && v.Value == "test_value(should_be_found)");

        configValues.ShouldContain(v => v.Key == "ParentKey:ChildKey");

        configValues.ShouldContain(v => v.Key == "ParentKey:ChildKey" &&
                                        v.Value == "ValueData");
    }
    [Fact]
    public void TestInfisicalSecretsProvider_ShouldLoadSecretsFor_Production()
    {
        // Arrange
        var configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Infisical:Enabled"]       = "true",
            ["Infisical:ProjectID"]     = "6aea8d7a-a4e2-41df-96f7-a5fa20d9853f",
            ["Infisical:Environment"]   = "prod",
            ["Infisical:VaultClientID"] = "d90475b4-144d-444e-9fde-15461c21e4b1",
            ["Infisical:VaultClientSecret"] =
                "d41ba8e700cea53dc4b78d609fd24b01c4cafa426dc7e0d1b23fe003d81b1023"
        });

        // Act
        configurationBuilder.AddInfisicalSecretsProvider();
        var configuration = configurationBuilder.Build();

        // Assert
        var configValues = configuration.AsEnumerable().ToList();

        configValues.ShouldContain(v => v.Key == "ConnectionStrings:DefaultConnectionStringForTesting");

        configValues.ShouldContain(v => v.Key == "ConnectionStrings:DefaultConnectionStringForTesting" && v.Value == "test_value(should_be_found_and_is_Prod)");
    }
}
