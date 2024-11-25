using Newtonsoft.Json;

namespace DotNetBrightener.Plugins.EventPubSub.AzureServiceBus.Native.Extensions;

internal static class JsonConfig
{
    public static readonly JsonSerializerSettings SerializeOptions = new()
    {
        ContractResolver      = new CamelCaseDictionaryKeysContractResolver(),
        NullValueHandling     = NullValueHandling.Ignore,
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
    };

    public static readonly JsonSerializerSettings DeserializeOptions = new()
    {
        ContractResolver  = new CamelCaseDictionaryKeysContractResolver(),
        NullValueHandling = NullValueHandling.Ignore
    };
}