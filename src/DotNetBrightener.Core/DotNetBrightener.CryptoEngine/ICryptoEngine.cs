using System;
using DotNetBrightener.CryptoEngine.Loaders;
using DotNetBrightener.CryptoEngine.Options;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;

namespace DotNetBrightener.CryptoEngine
{
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

    public class DefaultCryptoEngine : ICryptoEngine
    {
        private          bool                        _isInitialized;
        private          string                      _publicKey;
        private          string                      _privateKey;
        private readonly IEnumerable<IRSAKeysLoader> _rsaKeysLoaders;
        private readonly CryptoEngineConfiguration   _cryptoConfig;

        public DefaultCryptoEngine(IEnumerable<IRSAKeysLoader>         rsaKeysLoaders,
                                   IOptions<CryptoEngineConfiguration> cryptoConfig)
        {
            _rsaKeysLoaders = rsaKeysLoaders;
            _cryptoConfig   = cryptoConfig.Value;
        }

        public void Initialize()
        {
            if (_isInitialized)
                return;

            var rsaKeyLoader = _rsaKeysLoaders.FirstOrDefault(_ => _.LoaderName == _cryptoConfig.RsaKeyLoader);

            if (rsaKeyLoader == null)
            {
                throw new
                    InvalidOperationException("No RSA Key Loader configured. Please configured it using CryptoEngineConfiguration.RsaKeyLoader settings");
            }

            var keyPair = rsaKeyLoader.LoadOrInitializeKeyPair();
            _publicKey  = keyPair.Item1;
            _privateKey = keyPair.Item2;

            _isInitialized = true;
        }

        public string GetPublicKey()
        {
            return _publicKey;
        }

        public string EncryptText(string textToEncrypt)
        {
            EnsureInitialized();

            return EncryptText(textToEncrypt, _publicKey);
        }

        public string DecryptText(string cipherText)
        {
            EnsureInitialized();

            return DecryptText(cipherText, _privateKey);
        }

        public string EncryptText(string textToEncrypt, string publicKey)
        {
            return RsaCryptoEngine.EncryptString(textToEncrypt, publicKey);
        }

        public string DecryptText(string cipherText, string privateKey)
        {
            return RsaCryptoEngine.DecryptString(cipherText, privateKey);
        }

        public string SignData(string message)
        {
            return SignData(message, _privateKey);
        }

        public string SignData(string message, string privateKey)
        {
            return RsaCryptoEngine.SignData(message, privateKey);
        }

        public bool VerifyData(string message, string signature)
        {
            return VerifyData(message, signature, _publicKey);
        }

        public bool VerifyData(string message, string signature, string publicKey)
        {
            return RsaCryptoEngine.VerifyData(message, signature, publicKey);
        }

        private void EnsureInitialized()
        {
            if (!_isInitialized)
            {
                Initialize();
            }
        }
    }
}