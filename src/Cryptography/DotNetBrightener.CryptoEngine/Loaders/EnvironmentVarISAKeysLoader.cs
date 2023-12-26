using System;
using System.Security.Cryptography;
using DotNetBrightener.CryptoEngine.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace DotNetBrightener.CryptoEngine.Loaders;

public class EnvironmentVarISAKeysLoader : IRSAKeysLoader
{
    private readonly IConfiguration            _configuration;
    private readonly CryptoEngineConfiguration _cryptoConfig;

    public string LoaderName => "EnvVarLoader";

    public EnvironmentVarISAKeysLoader(IConfiguration                      configuration,
                                       IOptions<CryptoEngineConfiguration> cryptoConfig)
    {
        _configuration = configuration;
        _cryptoConfig  = cryptoConfig.Value;
    }

    public Tuple<string, string> LoadOrInitializeKeyPair()
    {
        var privateKeyValueFromEnvVar = Environment.GetEnvironmentVariable(_cryptoConfig.RsaEnvironmentVariableName) ??
                                        _configuration[_cryptoConfig.RsaEnvironmentVariableName];

        if (string.IsNullOrEmpty(privateKeyValueFromEnvVar))
        {
            var keyPair = RsaCryptoEngine.GenerateKeyPair();

            throw new
                CryptographicException($"Private Key for RSA Crypto Engine is not configured. Please add private key value below to Environment Variable with name '${_cryptoConfig.RsaEnvironmentVariableName}': {keyPair.Item2}");
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