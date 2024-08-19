using System.Security.Cryptography;
using System.Text;

namespace DotNetBrightener.CryptoEngine;

public static class AesCryptoEngine
{
    /// <summary>
    ///     Encrypts a string using AES encryption
    /// </summary>
    /// <param name="textToEncrypt">
    ///     The plain text to encrypt
    /// </param>
    /// <param name="encryptionKey">
    ///     The key to use for encryption, maximum length is 32 characters
    /// </param>
    /// <returns>
    ///     Encrypted string
    /// </returns>
    /// <exception cref="ArgumentException">
    ///     Thrown if the encryption key is null or empty, or exceeds 32 characters
    /// </exception>
    public static string Encrypt(string textToEncrypt, string encryptionKey)
    {
        if (string.IsNullOrEmpty(encryptionKey))
            throw new ArgumentException("Encryption key cannot be null or empty.");

        if (encryptionKey.Length > 32)
            throw new ArgumentException("Encryption key cannot be longer than 32 characters.");

        int    keySize = encryptionKey.Length <= 16 ? 16 : encryptionKey.Length <= 24 ? 24 : 32;
        byte[] Key     = new byte[keySize];
        Array.Copy(Encoding.UTF8.GetBytes(encryptionKey.PadRight(Key.Length)), Key, Key.Length);

        byte[] array;

        using (Aes aes = Aes.Create())
        {
            aes.KeySize = keySize * 8; // Set the key size in bits

            aes.GenerateIV();
            byte[] iv = aes.IV;

            ICryptoTransform encryptor = aes.CreateEncryptor(Key, iv);

            using (MemoryStream memoryStream = new())
            {
                using (CryptoStream cryptoStream = new(memoryStream, encryptor, CryptoStreamMode.Write))
                {
                    using (StreamWriter streamWriter = new(cryptoStream))
                    {
                        streamWriter.Write(textToEncrypt);
                    }

                    array = memoryStream.ToArray();
                }
            }

            byte[] combinedArray = new byte[iv.Length + array.Length];
            Array.Copy(iv, 0, combinedArray, 0, iv.Length);
            Array.Copy(array, 0, combinedArray, iv.Length, array.Length);

            return Convert.ToBase64String(combinedArray);
        }
    }

    /// <summary>
    ///    Decrypts a string using AES encryption
    /// </summary>
    /// <param name="textToDecrypt">
    ///     The encrypted text to decrypt
    /// </param>
    /// <param name="encryptionKey">
    ///     The key to use for decryption, maximum length is 32 characters
    /// </param>
    /// <returns>
    ///     The decrypted string
    /// </returns>
    /// <exception cref="ArgumentException">
    ///     Thrown if the encryption key is null or empty, or exceeds 32 characters
    /// </exception>
    public static string Decrypt(string textToDecrypt, string encryptionKey)
    {
        if (string.IsNullOrEmpty(encryptionKey))
            throw new ArgumentException("Encryption key cannot be null or empty.");

        if (encryptionKey.Length > 32)
            throw new ArgumentException("Encryption key cannot be longer than 32 characters.");

        int    keySize = encryptionKey.Length <= 16 ? 16 : encryptionKey.Length <= 24 ? 24 : 32;
        byte[] Key     = new byte[keySize];
        Array.Copy(Encoding.UTF8.GetBytes(encryptionKey.PadRight(Key.Length)), Key, Key.Length);

        byte[] fullCipher = Convert.FromBase64String(textToDecrypt);
        byte[] iv         = new byte[16];
        byte[] cipherText = new byte[fullCipher.Length - iv.Length];

        Array.Copy(fullCipher, iv, iv.Length);
        Array.Copy(fullCipher, iv.Length, cipherText, 0, cipherText.Length);

        using (Aes aes = Aes.Create())
        {
            aes.KeySize = keySize * 8; // Set the key size in bits
            ICryptoTransform decryptor = aes.CreateDecryptor(Key, iv);

            using (MemoryStream memoryStream = new(cipherText))
            {
                using (CryptoStream cryptoStream = new(memoryStream, decryptor, CryptoStreamMode.Read))
                {
                    using (StreamReader streamReader = new(cryptoStream))
                    {
                        return streamReader.ReadToEnd();
                    }
                }
            }
        }
    }

    /// <summary>
    ///     Tries to decrypt a string using AES encryption
    /// </summary>
    /// <param name="textToDecrypt">
    ///     The encrypted text to decrypt
    /// </param>
    /// <param name="output">
    ///     The decrypted string
    /// </param>
    /// <param name="encryptionKey">
    ///     The key to use for decryption, maximum length is 32 characters
    /// </param>
    /// <returns>
    ///     <c>true</c> if the decryption was successful; otherwise, <c>false</c>
    /// </returns>
    public static bool TryDecrypt(string textToDecrypt, out string output, string encryptionKey)
    {
        try
        {
            output = Decrypt(textToDecrypt, encryptionKey);

            return true;
        }
        catch (Exception)
        {
            output = string.Empty;

            return false;
        }
    }
}