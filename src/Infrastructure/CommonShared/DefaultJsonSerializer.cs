using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace WebApp.CommonShared;

public static class DefaultJsonSerializer
{
    public static readonly JsonSerializerSettings DefaultJsonSerializerSettings = new()
    {
        ContractResolver      = new CamelCasePropertyNamesContractResolver(),
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
    };
}