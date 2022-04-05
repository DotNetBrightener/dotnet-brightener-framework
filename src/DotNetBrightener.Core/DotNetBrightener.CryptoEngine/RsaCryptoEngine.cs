using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;

namespace DotNetBrightener.CryptoEngine
{
    /// <summary>
    ///     Provides methods to support encrypting/decrypting files/strings
    /// </summary>
    public static class RsaCryptoEngine
    {
        public static RSACryptoServiceProvider ImportFromXml(this RSACryptoServiceProvider csp, string xmlContent)
        {
            RSAParameters parameters = new();

            XmlDocument xmlDoc = new();
            xmlDoc.LoadXml(xmlContent);

            if (xmlDoc.DocumentElement!.Name.Equals(nameof(RSAKeyValue)))
            {
                xmlContent = xmlContent.Replace("<RSAKeyValue", "<RSAParameters")
                                       .Replace("</RSAKeyValue", "</RSAParameters");

                xmlDoc.LoadXml(xmlContent);
            }

            if (xmlDoc.DocumentElement!.Name.Equals(nameof(RSAParameters)))
            {
                parameters = (RSAParameters) new XmlSerializer(typeof(RSAParameters)).Deserialize(new StringReader(xmlContent));
            }

            csp.ImportParameters(parameters);

            return csp;
        }

        public static RSACryptoServiceProvider ImportFromXml(string xmlContent)
        {
            return new RSACryptoServiceProvider().ImportFromXml(xmlContent);
        }

        public static bool TryImportFromXml(String xmlContent, out RSACryptoServiceProvider cryptoEngine)
        {
            try
            {
                cryptoEngine = ImportFromXml(xmlContent);

                return true;
            }
            catch
            {
                cryptoEngine = null;

                return false;
            }
        }

        /// <summary>
        ///     Import OpenSSH PEM private key string into MS RSACryptoServiceProvider
        /// </summary>
        /// <param name="pem"></param>
        /// <returns></returns>
        public static RSACryptoServiceProvider ImportPemPrivateKey(string pem)
        {
            string reformattedPem = pem;
            if (!reformattedPem.StartsWith("-----BEGIN RSA PRIVATE KEY-----\n"))
            {
                reformattedPem = "-----BEGIN RSA PRIVATE KEY-----\n";

                for (var i = 0; i < pem.Length; i += 64)
                {
                    reformattedPem += pem.Substring(i, Math.Min(64, pem.Length - i));
                    reformattedPem += "\n";
                }

                reformattedPem += "-----END RSA PRIVATE KEY-----";
            }

            PemReader pr = new PemReader(new StringReader(reformattedPem));
            AsymmetricCipherKeyPair KeyPair = (AsymmetricCipherKeyPair) pr.ReadObject();
            RSAParameters rsaParams = DotNetUtilities.ToRSAParameters((RsaPrivateCrtKeyParameters) KeyPair.Private);

            RSACryptoServiceProvider csp = new RSACryptoServiceProvider(); // cspParams);
            csp.ImportParameters(rsaParams);

            return csp;
        }

        /// <summary>
        ///     Import OpenSSH PEM public key string into MS RSACryptoServiceProvider
        /// </summary>
        /// <param name="pem"></param>
        /// <returns></returns>
        public static RSACryptoServiceProvider ImportPemPublicKey(string pem)
        {
            string reformattedPem = pem;
            if (!reformattedPem.StartsWith("-----BEGIN PUBLIC KEY-----\n"))
            {
                reformattedPem = "-----BEGIN PUBLIC KEY-----\n";

                for (var i = 0; i < pem.Length; i += 64)
                {
                    reformattedPem += pem.Substring(i, Math.Min(64, pem.Length - i));
                    reformattedPem += "\n";
                }

                reformattedPem += "-----END PUBLIC KEY-----";
            }

            PemReader              pr        = new PemReader(new StringReader(reformattedPem));
            AsymmetricKeyParameter publicKey = (AsymmetricKeyParameter) pr.ReadObject();
            RSAParameters          rsaParams = DotNetUtilities.ToRSAParameters((RsaKeyParameters) publicKey);

            RSACryptoServiceProvider csp = new RSACryptoServiceProvider(); // cspParams);
            csp.ImportParameters(rsaParams);

            return csp;
        }

        public static string ExportPrivateKeyToPem(this RSACryptoServiceProvider csp)
        {
            StringWriter outputStream = new StringWriter();

            if (csp.PublicOnly)
                throw new ArgumentException("CSP does not contain a private key", "csp");

            var parameters = csp.ExportParameters(true);

            using (var stream = new MemoryStream())
            {
                var writer = new BinaryWriter(stream);
                writer.Write((byte) 0x30); // SEQUENCE

                using (var innerStream = new MemoryStream())
                {
                    var innerWriter = new BinaryWriter(innerStream);
                    EncodeIntegerBigEndian(innerWriter,
                                           new byte [ ]
                                           {
                                               0x00
                                           }); // Version
                    EncodeIntegerBigEndian(innerWriter, parameters.Modulus);
                    EncodeIntegerBigEndian(innerWriter, parameters.Exponent);
                    EncodeIntegerBigEndian(innerWriter, parameters.D);
                    EncodeIntegerBigEndian(innerWriter, parameters.P);
                    EncodeIntegerBigEndian(innerWriter, parameters.Q);
                    EncodeIntegerBigEndian(innerWriter, parameters.DP);
                    EncodeIntegerBigEndian(innerWriter, parameters.DQ);
                    EncodeIntegerBigEndian(innerWriter, parameters.InverseQ);
                    var length = (int) innerStream.Length;
                    EncodeLength(writer, length);
                    writer.Write(innerStream.GetBuffer(), 0, length);
                }

                var base64 = Convert.ToBase64String(stream.GetBuffer(), 0, (int) stream.Length).ToCharArray();
                // WriteLine terminates with \r\n, we want only \n
                outputStream.Write("-----BEGIN RSA PRIVATE KEY-----\n");

                // Output as Base64 with lines chopped at 64 characters
                for (var i = 0; i < base64.Length; i += 64)
                {
                    outputStream.Write(base64, i, Math.Min(64, base64.Length - i));
                    outputStream.Write("\n");
                }

                outputStream.Write("-----END RSA PRIVATE KEY-----");
            }

            return outputStream.ToString();
        }

        public static string ExportPublicKeyToPem(this RSACryptoServiceProvider csp)
        {
            StringWriter outputStream = new StringWriter();
            var          parameters   = csp.ExportParameters(false);

            using (var stream = new MemoryStream())
            {
                var writer = new BinaryWriter(stream);
                writer.Write((byte) 0x30); // SEQUENCE

                using (var innerStream = new MemoryStream())
                {
                    var innerWriter = new BinaryWriter(innerStream);
                    innerWriter.Write((byte) 0x30); // SEQUENCE
                    EncodeLength(innerWriter, 13);
                    innerWriter.Write((byte) 0x06); // OBJECT IDENTIFIER
                    var rsaEncryptionOid = new byte [ ]
                                           {
                                               0x2a, 0x86, 0x48, 0x86, 0xf7, 0x0d, 0x01, 0x01, 0x01
                                           };
                    EncodeLength(innerWriter, rsaEncryptionOid.Length);
                    innerWriter.Write(rsaEncryptionOid);
                    innerWriter.Write((byte) 0x05); // NULL
                    EncodeLength(innerWriter, 0);
                    innerWriter.Write((byte) 0x03); // BIT STRING

                    using (var bitStringStream = new MemoryStream())
                    {
                        var bitStringWriter = new BinaryWriter(bitStringStream);
                        bitStringWriter.Write((byte) 0x00); // # of unused bits
                        bitStringWriter.Write((byte) 0x30); // SEQUENCE

                        using (var paramsStream = new MemoryStream())
                        {
                            var paramsWriter = new BinaryWriter(paramsStream);
                            EncodeIntegerBigEndian(paramsWriter, parameters.Modulus);  // Modulus
                            EncodeIntegerBigEndian(paramsWriter, parameters.Exponent); // Exponent
                            var paramsLength = (int) paramsStream.Length;
                            EncodeLength(bitStringWriter, paramsLength);
                            bitStringWriter.Write(paramsStream.GetBuffer(), 0, paramsLength);
                        }

                        var bitStringLength = (int) bitStringStream.Length;
                        EncodeLength(innerWriter, bitStringLength);
                        innerWriter.Write(bitStringStream.GetBuffer(), 0, bitStringLength);
                    }

                    var length = (int) innerStream.Length;
                    EncodeLength(writer, length);
                    writer.Write(innerStream.GetBuffer(), 0, length);
                }

                var base64 = Convert.ToBase64String(stream.GetBuffer(), 0, (int) stream.Length).ToCharArray();
                // WriteLine terminates with \r\n, we want only \n
                outputStream.Write("-----BEGIN PUBLIC KEY-----\n");

                for (var i = 0; i < base64.Length; i += 64)
                {
                    outputStream.Write(base64, i, Math.Min(64, base64.Length - i));
                    outputStream.Write("\n");
                }

                outputStream.Write("-----END PUBLIC KEY-----");
            }

            return outputStream.ToString();
        }

        /// <summary>
        /// https://stackoverflow.com/a/23739932/2860309
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="length"></param>
        private static void EncodeLength(BinaryWriter stream, int length)
        {
            if (length < 0)
                throw new ArgumentOutOfRangeException("length", "Length must be non-negative");

            if (length < 0x80)
            {
                // Short form
                stream.Write((byte) length);
            }
            else
            {
                // Long form
                var temp          = length;
                var bytesRequired = 0;

                while (temp > 0)
                {
                    temp >>= 8;
                    bytesRequired++;
                }

                stream.Write((byte) (bytesRequired | 0x80));

                for (var i = bytesRequired - 1; i >= 0; i--)
                {
                    stream.Write((byte) (length >> (8 * i) & 0xff));
                }
            }
        }

        /// <summary>
        /// https://stackoverflow.com/a/23739932/2860309
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="value"></param>
        /// <param name="forceUnsigned"></param>
        private static void EncodeIntegerBigEndian(BinaryWriter stream, byte [ ] value, bool forceUnsigned = true)
        {
            stream.Write((byte) 0x02); // INTEGER
            var prefixZeros = 0;

            for (var i = 0; i < value.Length; i++)
            {
                if (value [i] != 0)
                    break;

                prefixZeros++;
            }

            if (value.Length - prefixZeros == 0)
            {
                EncodeLength(stream, 1);
                stream.Write((byte) 0);
            }
            else
            {
                if (forceUnsigned && value [prefixZeros] > 0x7f)
                {
                    // Add a prefix zero to force unsigned if the MSB is 1
                    EncodeLength(stream, value.Length - prefixZeros + 1);
                    stream.Write((byte) 0);
                }
                else
                {
                    EncodeLength(stream, value.Length - prefixZeros);
                }

                for (var i = prefixZeros; i < value.Length; i++)
                {
                    stream.Write(value [i]);
                }
            }
        }

        /// <summary>
        ///     Generates a pair of public/private keys to use with encryption
        /// </summary>
        /// <returns>
        ///     A <see cref="Tuple{T1, T2}"/> of 2 <see cref="string"/>s which represent public and private key
        /// </returns>
        public static Tuple<string, string> GenerateKeyPair()
        {
            var csp = new RSACryptoServiceProvider(2048);

            var publicKey = csp.ExportPublicKeyToPem();
            var privateKey = csp.ExportPrivateKeyToPem();

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
            if (!TryImportFromXml(publicKey, out var csp))
            {
                csp = ImportPemPublicKey(publicKey);
            }

            if (csp == null)
                throw new InvalidOperationException("Cannot initiate crypto provider using provided key");

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
            if (!TryImportFromXml(privateKey, out var csp))
            {
                csp = ImportPemPrivateKey(privateKey);
            }

            if (csp == null)
                throw new InvalidOperationException("Cannot initiate crypto provider using provided key");

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
        public static void EncryptFile(string filePath, string publicKey, string outputFile = null)
        {
            if (!TryImportFromXml(publicKey, out var csp))
            {
                csp = ImportPemPublicKey(publicKey);
            }

            if (csp == null)
                throw new InvalidOperationException("Cannot initiate crypto provider using provided key");

            var bytesPlainTextData = File.ReadAllBytes(filePath);

            var bytesCipherText = csp.Encrypt(bytesPlainTextData, false);

            var encryptedText = Convert.ToBase64String(bytesCipherText);
            File.WriteAllText(outputFile ?? filePath, encryptedText);
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
        public static void DecryptFile(string filePath, string privateKey, string outputFile = null)
        {
            if (!TryImportFromXml(privateKey, out var csp))
            {
                csp = ImportPemPrivateKey(privateKey);
            }

            if (csp == null)
                throw new InvalidOperationException("Cannot initiate crypto provider using provided key");

            var encryptedText = File.ReadAllText(filePath);

            var bytesCipherText = Convert.FromBase64String(encryptedText);

            var bytesPlainTextData = csp.Decrypt(bytesCipherText, false);

            File.WriteAllBytes(outputFile ?? filePath, bytesPlainTextData);
        }

        public static string SignData(string message, string privateKey)
        {
            byte [ ] signedBytes;

            using (var rsa = ImportPemPrivateKey(privateKey))
            {
                var      encoder      = new UTF8Encoding();
                byte [ ] originalData = encoder.GetBytes(message);

                try
                {
                    signedBytes = rsa.SignData(originalData, CryptoConfig.MapNameToOID("SHA512"));
                }
                catch (CryptographicException e)
                {
                    Console.WriteLine(e.Message);

                    return null;
                }
                finally
                {
                    rsa.PersistKeyInCsp = false;
                }
            }

            return Convert.ToBase64String(signedBytes);
        }

        public static bool VerifyData(string message, string signature, string publicKey)
        {
            bool success = false;
            using (var rsa = ImportPemPublicKey(publicKey))
            {
                var    encoder       = new UTF8Encoding();
                byte[] bytesToVerify = encoder.GetBytes(message);
                byte[] signedBytes   = Convert.FromBase64String(signature);
                try
                {
                    SHA512Managed Hash = new SHA512Managed();

                    byte[] hashedData = Hash.ComputeHash(signedBytes);

                    success = rsa.VerifyData(bytesToVerify, CryptoConfig.MapNameToOID("SHA512"), signedBytes);
                }
                catch (CryptographicException e)
                {
                    Console.WriteLine(e.Message);
                }
                finally
                {
                    rsa.PersistKeyInCsp = false;
                }
            }
            return success;
        }
    }
}