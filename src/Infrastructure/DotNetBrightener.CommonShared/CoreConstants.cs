using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace DotNetBrightener.CommonShared;

public class CoreConstants
{
    public static readonly JsonSerializerSettings DefaultJsonSerializerSettings = new JsonSerializerSettings
    {
        ContractResolver      = new CamelCasePropertyNamesContractResolver(),
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
    };
}

public class CoreSettings
{
    public string PublicSiteUrl { get; set; }

    public string BackendSiteUrl { get; set; }
}