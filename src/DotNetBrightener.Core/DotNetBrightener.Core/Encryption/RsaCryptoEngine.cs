using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Serialization;

namespace DotNetBrightener.Core.Encryption
{
    /// <summary>
    ///     Provides methods to support encrypting/decrypting files/strings
    /// </summary>
    public class RsaCryptoEngine
    {
        private static XmlSerializer _xmlSerializer;

        private static XmlSerializer CryptoEngineSerializer => _xmlSerializer ??= new XmlSerializer(typeof(RSAParameters));

        /// <summary>
        ///     Generates a pair of public/private keys to use with encryption
        /// </summary>
        /// <returns>
        ///     A <see cref="Tuple{T1, T2}"/> of 2 <see cref="string"/>s which represent public and private key
        /// </returns>
        public static Tuple<string, string> MakeKeyPair()
        {
            var csp = new RSACryptoServiceProvider(2048);
            
            string publicKey, privateKey;

            using (var sw = new StringWriter())
            {
                CryptoEngineSerializer.Serialize(sw, csp.ExportParameters(false));
                publicKey = sw.ToString();
            }

            using (var sw = new StringWriter())
            {
                CryptoEngineSerializer.Serialize(sw, csp.ExportParameters(true));
                privateKey = sw.ToString();
            }

            return new Tuple<string, string>(publicKey, privateKey);
        }

        /// <summary>
        ///     Validates a pair of public and private key
        /// </summary>
        /// <param name="publicKey">The public key</param>
        /// <param name="privateKey">The private key</param>
        /// <returns>
        ///     <c>true</c> if the keys are paired and valid. Otherwise, <c>false</c>
        /// </returns>
        public static bool ValidateKeyPair(string publicKey, string privateKey)
        {
            var testString = "This is string to validate, plus some guid: " + Guid.NewGuid();
            try
            {
                var encryptedString = EncryptString(testString, publicKey);
                var decryptedString = DecryptString(encryptedString, privateKey);

                return string.Equals(decryptedString, testString);
            }
            catch (Exception exception)
            {
                return false;
            }
        }

        /// <summary>
        ///     Generates an encrypted string from the given one and the public key
        /// </summary>
        /// <param name="stringToEncrypt">
        ///     The <see cref="string" /> to encrypt
        /// </param>
        /// <param name="publicKey">
        ///     The public key to use for encryption
        /// </param>
        /// <returns>
        ///     The encrypted string
        /// </returns>
        public static string EncryptString(string stringToEncrypt, string publicKey)
        {
            var csp = new RSACryptoServiceProvider();
            csp.ImportParameters((RSAParameters)CryptoEngineSerializer.Deserialize(new StringReader(publicKey)));

            var bytesPlainTextData = Encoding.Unicode.GetBytes(stringToEncrypt);

            var bytesCipherText = csp.Encrypt(bytesPlainTextData, false);
            
            var encryptedText = Convert.ToBase64String(bytesCipherText);
            return encryptedText;
        }

        /// <summary>
        ///     Decrypts an encrypted string using the private key
        /// </summary>
        /// <param name="encryptedString">
        ///     The encrypted <see cref="string"/>
        /// </param>
        /// <param name="privateKey">
        ///     The private key which is paired with the public key that was used to encrypt the <see cref="encryptedString" />
        /// </param>
        /// <returns>
        ///     The descrypted string
        /// </returns>
        public static string DecryptString(string encryptedString, string privateKey)
        {
            var csp = new RSACryptoServiceProvider();
            csp.ImportParameters((RSAParameters)CryptoEngineSerializer.Deserialize(new StringReader(privateKey)));

            var bytesCipherText = Convert.FromBase64String(encryptedString);

            var bytesPlainTextData = csp.Decrypt(bytesCipherText, false);

            return Encoding.Unicode.GetString(bytesPlainTextData);
        }

        /// <summary>
        ///     Generates an encrypted file from the given path and the public key
        /// </summary>
        /// <param name="filePath">
        ///     The path to the file to encrypt
        /// </param>
        /// <param name="publicKey">
        ///     The public key to use for encryption
        /// </param>
        public static void EncryptFile(string filePath, string publicKey)
        {
            var csp = new RSACryptoServiceProvider();
            csp.ImportParameters((RSAParameters)CryptoEngineSerializer.Deserialize(new StringReader(publicKey)));

            var bytesPlainTextData = File.ReadAllBytes(filePath);
            
            var bytesCipherText = csp.Encrypt(bytesPlainTextData, false);
            
            var encryptedText = Convert.ToBase64String(bytesCipherText);
            File.WriteAllText(filePath, encryptedText);
        }

        /// <summary>
        ///     Decrypts an encrypted file using the private key
        /// </summary>
        /// <param name="filePath">
        ///     The path to the encrypted file
        /// </param>
        /// <param name="privateKey">
        ///     The private key which is paired with the public key that was used to encrypt the file
        /// </param>
        public static void DecryptFile(string filePath, string privateKey)
        {
            var csp = new RSACryptoServiceProvider();
            csp.ImportParameters((RSAParameters)CryptoEngineSerializer.Deserialize(new StringReader(privateKey)));

            var encryptedText = File.ReadAllText(filePath);
            
            var bytesCipherText = Convert.FromBase64String(encryptedText);

            var bytesPlainTextData = csp.Decrypt(bytesCipherText, false);

            File.WriteAllBytes(filePath, bytesPlainTextData);
        }
    }
}