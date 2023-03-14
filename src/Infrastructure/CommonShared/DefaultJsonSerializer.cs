using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace DotNetBrightener.WebApp.CommonShared;

public class DefaultJsonSerializer
{
    public static readonly JsonSerializerSettings DefaultJsonSerializerSettings = new JsonSerializerSettings
    {
        ContractResolver      = new CamelCasePropertyNamesContractResolver(),
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
    };
}