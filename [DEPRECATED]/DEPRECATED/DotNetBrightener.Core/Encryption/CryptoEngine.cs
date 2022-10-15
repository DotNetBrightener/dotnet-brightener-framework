using System.IO;
using System.Security.Cryptography;
using DotNetBrightener.Core.IO;
using Microsoft.Extensions.FileProviders;

namespace DotNetBrightener.Core.Encryption;

public class CryptoEngine: ICryptoEngine
{
    private bool   _isInitialized;
    private string _publicKey;
    private string _privateKey;
        
    private const string PublicKeyFileName  = "public.key";
    private const string PrivateKeyFileName = "private.key";

    private readonly IConfigurationFilesProvider _configurationFilesProvider;

    public CryptoEngine(IConfigurationFilesProvider configurationFilesProvider)
    {
        _configurationFilesProvider = configurationFilesProvider;
    }

    public void Initialize()
    {
        void GenerateKeyPair(IFileInfo publicKeyFileInfo, IFileInfo privateKeyFileInfo)
        {
            var keyPair = RsaCryptoEngine.MakeKeyPair();
            _publicKey  = keyPair.Item1;
            _privateKey = keyPair.Item2;

            File.WriteAllText(publicKeyFileInfo.PhysicalPath, _publicKey);
            File.WriteAllText(privateKeyFileInfo.PhysicalPath, _privateKey);
        }

        if (_isInitialized)
            return;

        var publicKeyFile  = _configurationFilesProvider.GetFileInfo(PublicKeyFileName);
        var privateKeyFile = _configurationFilesProvider.GetFileInfo(PrivateKeyFileName);

        // if both files do not exist
        if (!publicKeyFile.Exists && !privateKeyFile.Exists)
        {
            GenerateKeyPair(publicKeyFile, privateKeyFile);
        }
        // if both files exist
        else if (publicKeyFile.Exists && privateKeyFile.Exists)
        {
            _publicKey  = File.ReadAllText(publicKeyFile.PhysicalPath);
            _privateKey = File.ReadAllText(privateKeyFile.PhysicalPath);

            if (!RsaCryptoEngine.ValidateKeyPair(_publicKey, _privateKey))
            {
                throw new CryptographicException("The keys pair is not valid. The encryption will be failed.");
            }
        }
        // if any of the file exists while the other does not, probably the encryption is broken
        else
        {
            throw new CryptographicException("One of the keys for encryption is missing.");
        }

        _isInitialized = true;
    }

    public string EncryptText(string textToEncrypt)
    {
        EnsureInitialized();

        return RsaCryptoEngine.EncryptString(textToEncrypt, _publicKey);
    }

    public string DecryptText(string cipherText)
    {
        EnsureInitialized();

        return RsaCryptoEngine.DecryptString(cipherText, _privateKey);
    }

    private void EnsureInitialized()
    {
        if (!_isInitialized)
        {
            Initialize();
        }
    }
}