namespace DotNetBrightener.CryptoEngine;

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
    ///     Retrieves the system public key
    /// </summary>
    /// <returns>
    ///     The public key that the system uses
    /// </returns>
    string GetPublicKey();

    /// <summary>
    ///     Perform internal encryption operation over the given text, using the system key
    /// </summary>
    /// <param name="textToEncrypt">
    ///     The text to encrypt
    /// </param>
    /// <returns>
    ///     The encrypted string
    /// </returns>
    string EncryptText(string textToEncrypt);

    /// <summary>
    ///     Perform internal decryption operation over the given encrypted text, using the system key
    /// </summary>
    /// <param name="cipherText">
    ///     The text to decrypt
    /// </param>
    /// <returns>
    ///     The decrypted string
    /// </returns>
    string DecryptText(string cipherText);

    /// <summary>
    ///     Perform internal encryption operation over the given text, using the provided key
    /// </summary>
    /// <param name="textToEncrypt">
    ///     The text to encrypt
    /// </param>
    /// <returns>
    ///     The encrypted string
    /// </returns>
    string EncryptText(string textToEncrypt, string publicKey);

    /// <summary>
    ///     Perform internal decryption operation over the given encrypted text, using the provided key
    /// </summary>
    /// <param name="cipherText">
    ///     The text to decrypt
    /// </param>
    /// <returns>
    ///     The decrypted string
    /// </returns>
    string DecryptText(string cipherText, string privateKey);

    string SignData(string message);

    string SignData(string message, string privateKey);

    bool VerifyData(string message, string signature);

    bool VerifyData(string message, string signature, string publicKey);
}