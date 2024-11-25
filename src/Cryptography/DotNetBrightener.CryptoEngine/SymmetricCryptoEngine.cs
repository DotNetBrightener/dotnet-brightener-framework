namespace DotNetBrightener.CryptoEngine;

/// <summary>
///     Provides methods for symmetric encryption and decryption using TripleDES algorithm
/// </summary>
/// <remarks>
///     This class is considered as an old-fashioned way of doing encryption.
///     Consider moving to <see cref="AesCryptoEngine"/> instead.
///     Please note <see cref="AesCryptoEngine"/> is not compatible with this class.
/// <br />
/// <br />
///     This class will be removed in future versions.
/// </remarks>
[Obsolete("This is old-fashioned way of doing encryption. If you still want to use this, switch to TripleDesCryptoEngine instead. " +
          "New Aes algorithm is available in AesCryptoEngine class.")]
public static class SymmetricCryptoEngine
{
    public static string Encrypt(string textToEncrypt, string encryptionKey = "")
        => TripleDesCryptoEngine.Encrypt(textToEncrypt, encryptionKey);

    public static string Decrypt(string textToDecrypt, string encryptionKey = "")
        => TripleDesCryptoEngine.Decrypt(textToDecrypt, encryptionKey);

    public static bool TryDecrypt(string textToDecrypt, out string output, string encryptionKey)
        => TripleDesCryptoEngine.TryDecrypt(textToDecrypt, out output, encryptionKey);
}