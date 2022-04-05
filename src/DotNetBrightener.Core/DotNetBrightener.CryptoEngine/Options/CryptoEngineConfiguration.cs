namespace DotNetBrightener.CryptoEngine.Options
{
    public class CryptoEngineConfiguration
    {
        public string RsaKeyLoader { get; set; }

        public string RsaEnvironmentVariableName { get; set; } = "RSAPrivateKey";
    }
}
