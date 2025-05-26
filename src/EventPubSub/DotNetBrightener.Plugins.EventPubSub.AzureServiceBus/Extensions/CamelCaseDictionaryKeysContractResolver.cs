using Newtonsoft.Json.Serialization;

namespace DotNetBrightener.Plugins.EventPubSub.AzureServiceBus.Extensions;

internal class CamelCaseDictionaryKeysContractResolver : CamelCasePropertyNamesContractResolver
{
    protected override string ResolveDictionaryKey(string dictionaryKey)
    {
        return base.ResolvePropertyName(dictionaryKey);
    }
}