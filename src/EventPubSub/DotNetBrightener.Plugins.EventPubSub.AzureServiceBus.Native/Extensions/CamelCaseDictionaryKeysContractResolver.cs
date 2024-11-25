using Newtonsoft.Json.Serialization;

namespace DotNetBrightener.Plugins.EventPubSub.AzureServiceBus.Native.Extensions;

internal class CamelCaseDictionaryKeysContractResolver : CamelCasePropertyNamesContractResolver
{
    protected override string ResolveDictionaryKey(string dictionaryKey)
    {
        return base.ResolvePropertyName(dictionaryKey);
    }
}