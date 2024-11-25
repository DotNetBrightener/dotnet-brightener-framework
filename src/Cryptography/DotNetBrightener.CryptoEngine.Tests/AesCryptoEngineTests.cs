using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace DotNetBrightener.CryptoEngine.Tests;

public class AesCryptoEngineTests(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public void TestEncryption_ShouldThrowException()
    {
        var encryptionKey = PasswordUtilities.GenerateRandomString(length: 33, includeSymbols: true);

        Action action = () => AesCryptoEngine.Encrypt("whatever string", encryptionKey);

        action.Should()
              .Throw<ArgumentException>()
              .WithMessage("Encryption key cannot be longer than 32 characters.");
    }

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
            var encryptedText = AesCryptoEngine.Encrypt(textToEncrypt, encryptionKey);

            var decryptedText = AesCryptoEngine.Decrypt(encryptedText, encryptionKey);

            textToEncrypt.Should().Be(decryptedText);
        }
    }
}