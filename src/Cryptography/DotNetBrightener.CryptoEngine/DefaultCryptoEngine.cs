using DotNetBrightener.CryptoEngine.Loaders;
using DotNetBrightener.CryptoEngine.Options;
using Microsoft.Extensions.Options;

namespace DotNetBrightener.CryptoEngine;

public class DefaultCryptoEngine(
    IEnumerable<IRSAKeysLoader>         rsaKeysLoaders,
    IOptions<CryptoEngineConfiguration> cryptoConfig)
    : ICryptoEngine
{
    private          bool                      _isInitialized;
    private          string                    _publicKey;
    private          string                    _privateKey;
    private readonly CryptoEngineConfiguration _cryptoConfig   = cryptoConfig.Value;

    public void Initialize()
    {
        if (_isInitialized)
            return;

        var rsaKeyLoader = rsaKeysLoaders.FirstOrDefault(_ => _.LoaderName == _cryptoConfig.RsaKeyLoader);

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