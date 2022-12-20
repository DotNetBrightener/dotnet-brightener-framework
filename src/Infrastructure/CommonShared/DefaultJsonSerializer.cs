using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace DotNetBrightener.CommonShared;

public class DefaultJsonSerializer
{
    public static readonly JsonSerializerSettings DefaultJsonSerializerSettings = new JsonSerializerSettings
    {
        ContractResolver      = new CamelCasePropertyNamesContractResolver(),
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
    };
}