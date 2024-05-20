using System.Text.Json.Serialization;
using DotNetBrightener.Utils.MessageCompression;

namespace DotNetBrightener.SecuredApi;

public class SecuredApiResult
{
    [JsonExtensionData]
    public Dictionary<string, object> ResultData { get; set; } = null!;

    internal bool ShortCircuit { get; set; }

    public static SecuredApiResult FromPayload(object payload)
    {
        return new SecuredApiResult
        {
            ResultData = payload.ToPayload()
        };
    }

    public void ShortCircuitRequest()
    {
        ShortCircuit = true;
    }
}