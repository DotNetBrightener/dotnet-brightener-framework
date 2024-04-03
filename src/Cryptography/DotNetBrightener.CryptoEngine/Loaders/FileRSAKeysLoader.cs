using Microsoft.Extensions.FileProviders;
using System.Security.Cryptography;

namespace DotNetBrightener.CryptoEngine.Loaders;

public class FileRSAKeysLoader : IRSAKeysLoader
{
    private       string _publicKey;
    private       string _privateKey;
    private const string PublicKeyFileName  = "public.key";
    private const string PrivateKeyFileName = "private.key";

    private readonly IFileProvider _configurationFilesProvider;

    public FileRSAKeysLoader(string rootPath)
    {
        var encryptionConfigFolder = Path.Combine(rootPath, "enc_keys");

        if (!Directory.Exists(encryptionConfigFolder))
        {
            Directory.CreateDirectory(encryptionConfigFolder);
        }

        _configurationFilesProvider = new PhysicalFileProvider(encryptionConfigFolder);
    }

    public string LoaderName => "FileLoader";

    public Tuple<string, string> LoadOrInitializeKeyPair()
    {
        var publicKeyFile  = _configurationFilesProvider.GetFileInfo(PublicKeyFileName);
        var privateKeyFile = _configurationFilesProvider.GetFileInfo(PrivateKeyFileName);

        // if both files do not exist
        if (!publicKeyFile.Exists &&
            !privateKeyFile.Exists)
        {
            var keyPair = RsaCryptoEngine.GenerateKeyPair();
            _publicKey  = keyPair.Item1;
            _privateKey = keyPair.Item2;

            File.WriteAllText(publicKeyFile.PhysicalPath, _publicKey);
            File.WriteAllText(privateKeyFile.PhysicalPath, _privateKey);
        }
        // if only private key file exist
        else if (privateKeyFile.Exists && !publicKeyFile.Exists)
        {
            _privateKey = File.ReadAllText(privateKeyFile.PhysicalPath);
            RSACryptoServiceProvider csp;
                
            if (_privateKey.Contains("<?xml "))
            {
                csp         = new RSACryptoServiceProvider().ImportFromXml(_privateKey);
                _privateKey = csp.ExportPrivateKeyToPem();
                _publicKey  = csp.ExportPublicKeyToPem();
            }
            else
            {
                csp        = RsaCryptoEngine.ImportPemPrivateKey(_privateKey);
                _publicKey = csp.ExportPublicKeyToPem();
            }

            File.WriteAllText(publicKeyFile.PhysicalPath, _publicKey);
            File.WriteAllText(privateKeyFile.PhysicalPath, _privateKey);
        }
        // if both files exist
        else if (publicKeyFile.Exists &&
                 privateKeyFile.Exists)
        {
            _publicKey  = File.ReadAllText(publicKeyFile.PhysicalPath);
            _privateKey = File.ReadAllText(privateKeyFile.PhysicalPath);

            if (!RsaCryptoEngine.ValidateKeyPair(_publicKey, _privateKey))
            {
                throw new CryptographicException("The keys pair is not valid. The encryption will be failed.");
            }

            // convert the xml key to PEM
            if (_publicKey.Contains("<?xml ") &&
                _privateKey.Contains("<?xml "))
            {
                var csp = RsaCryptoEngine.ImportFromXml(_privateKey);
                _privateKey = csp.ExportPrivateKeyToPem();
                _publicKey  = csp.ExportPublicKeyToPem();

                File.WriteAllText(publicKeyFile.PhysicalPath, _publicKey);
                File.WriteAllText(privateKeyFile.PhysicalPath, _privateKey);
            }
        }
        // if only public key file exists while the other does not, probably the encryption is broken
        else
        {
            throw new CryptographicException("One of the keys for encryption is missing.");
        }

        return new Tuple<string, string>(_publicKey, _privateKey);
    }
}