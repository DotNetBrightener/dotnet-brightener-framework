using NUnit.Framework;

namespace DotNetBrightener.InfisicalVaultClient.Tests;

[TestFixture]
public class InfisicalSecretClientTest
{
    private InfisicalSecretClient _client;
    [SetUp]
    public void Setup()
    {
        _client = new InfisicalSecretClient(
                                            "https://dnbvault.dotnetbrightener.com",
 "st.645b62b73cd3b0b98b246586.5d1cee582c21c7b7f1975ee9ddf28113.2d000a08ea46f267e1d3e4ae6480b5ea");
    }

    [Test]
    public async Task TestLoadSecret()
    {
        var secretResult = await _client.RetrieveSecret("DbConnectionString", "645b5fd29f367616b4be6ac1");
    }

    [Test]
    public async Task TestLoadSecrets()
    {
        var secretResult = await _client.RetrieveSecrets("645b5fd29f367616b4be6ac1");
    }
}