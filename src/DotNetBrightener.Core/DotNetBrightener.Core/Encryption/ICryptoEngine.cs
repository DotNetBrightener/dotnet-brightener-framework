namespace DotNetBrightener.Core.Encryption;

/// <summary>
///     Represents the engine used for encrypting
/// </summary>
public interface ICryptoEngine
{
    /// <summary>
    ///     Initializes the crypto engine
    /// </summary>
    void Initialize();

    /// <summary>
    ///     Perform internal encryption operation over the given text
    /// </summary>
    /// <param name="textToEncrypt">
    ///     The text to encrypt
    /// </param>
    /// <returns>
    ///     The encrypted string
    /// </returns>
    string EncryptText(string textToEncrypt);

    /// <summary>
    ///     Perform internal decryption operation over the given encrypted text
    /// </summary>
    /// <param name="cipherText">
    ///     The text to decrypt
    /// </param>
    /// <returns>
    ///     The decrypted string
    /// </returns>
    string DecryptText(string cipherText);
}