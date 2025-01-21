using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace DotNetBrightener.CryptoEngine.Tests;

public class TripleDesCryptoEngineTests(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public void TestEncryption_ShouldDecryptSuccessfully()
    {
        var encryptionKey = PasswordUtilities.GenerateRandomString(includeSymbols: true);

        testOutputHelper.WriteLine("Encryption Key: " + encryptionKey);

        List<string> stringsToEncrypt =
        [
            "Hello, World!",
            "This is a test",
            "Another test string"
        ];

        foreach (var textToEncrypt in stringsToEncrypt)
        {
            var encryptedText = TripleDesCryptoEngine.Encrypt(textToEncrypt, encryptionKey);

            var decryptedText = TripleDesCryptoEngine.Decrypt(encryptedText, encryptionKey);

            textToEncrypt.ShouldBe(decryptedText);
        }
    }
}