using System.Text.Json.Serialization;

namespace DotNetBrightener.PushNotification.APN.Models;

internal class ApnResponse
{
    [JsonPropertyName("reason")]
    public string Reason { get; set; }

    [JsonPropertyName("timestamp")]
    public long? Timestamp { get; set; }
}

internal class ApnError
{
    public string DeviceToken { get; set; }
    public int StatusCode { get; set; }
    public string Reason { get; set; }
    public DateTime? Timestamp { get; set; }
    public string ApnsId { get; set; }
}
