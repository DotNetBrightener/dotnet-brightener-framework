using System.Security.Cryptography;
using DotNetBrightener.CryptoEngine.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace DotNetBrightener.CryptoEngine.Loaders;

public class EnvironmentVarISAKeysLoader(
    IConfiguration                      configuration,
    IOptions<CryptoEngineConfiguration> cryptoConfig)
    : IRSAKeysLoader
{
    private readonly CryptoEngineConfiguration _cryptoConfig  = cryptoConfig.Value;

    public string LoaderName => "EnvVarLoader";

    public Tuple<string, string> LoadOrInitializeKeyPair()
    {
        var privateKeyValueFromEnvVar = Environment.GetEnvironmentVariable(_cryptoConfig.RsaEnvironmentVariableName) ??
                                        configuration[_cryptoConfig.RsaEnvironmentVariableName];

        if (string.IsNullOrEmpty(privateKeyValueFromEnvVar))
        {
            var keyPair = RsaCryptoEngine.GenerateKeyPair(true);

            throw new
                CryptographicException($"Private Key for RSA Crypto Engine is not configured. " +
                                       $"Please add private key value below to Environment Variable with name '${_cryptoConfig.RsaEnvironmentVariableName}': {keyPair.Item2}");
        }

        var isXmlFormat = privateKeyValueFromEnvVar.Contains("<?xml ");

        RSACryptoServiceProvider csp = isXmlFormat
                                           ? RsaCryptoEngine.ImportFromXml(privateKeyValueFromEnvVar)
                                           : RsaCryptoEngine.ImportPemPrivateKey(privateKeyValueFromEnvVar);

        var publicKey  = csp.ExportPublicKeyToPem();
        var privateKey = csp.ExportPrivateKeyToPem();

        return new Tuple<string, string>(publicKey, privateKey);
    }
}