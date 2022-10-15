using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace DotNetBrightener.Core;

public class CoreConstants
{
    public const string TenantName = "TENANT_NAME";

    public const string CurrentCultureRequestKey = "CURRENT_CULTURE";

    public static readonly JsonSerializerSettings DefaultJsonSerializerSettings = new JsonSerializerSettings
    {
        ContractResolver = new DefaultContractResolver
        {
            NamingStrategy = new CamelCaseNamingStrategy()
        },
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
    };
}