using DotNetBrightener.CryptoEngine.Loaders;

namespace DotNetBrightener.CryptoEngine.Tests;

public class InMemoryRSAKeysLoader : IRSAKeysLoader
{
    public string LoaderName => "InMemory";

    public Tuple<string, string> LoadOrInitializeKeyPair()
    {
        var keyPair = RsaCryptoEngine.GenerateKeyPair(true);

        return keyPair;
    }
}