using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

public static class DefaultJsonSerializer
{
    public static readonly JsonSerializerSettings DefaultJsonSerializerSettings = new()
    {
        ContractResolver      = new CamelCasePropertyNamesContractResolver(),
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
    };
}