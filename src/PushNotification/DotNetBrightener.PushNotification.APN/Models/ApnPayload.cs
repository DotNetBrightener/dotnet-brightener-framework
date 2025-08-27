using System.Text.Json.Serialization;

namespace DotNetBrightener.PushNotification.APN.Models;

internal class ApnPayload
{
    [JsonPropertyName("aps")]
    public ApnAps Aps { get; set; }

    [JsonExtensionData]
    public Dictionary<string, object> CustomData { get; set; }
}

internal class ApnAps
{
    [JsonPropertyName("alert")]
    public ApnAlert Alert { get; set; }

    [JsonPropertyName("badge")]
    public int? Badge { get; set; }

    [JsonPropertyName("sound")]
    public string Sound { get; set; } = "default";

    [JsonPropertyName("content-available")]
    public int? ContentAvailable { get; set; }

    [JsonPropertyName("mutable-content")]
    public int? MutableContent { get; set; }

    [JsonPropertyName("category")]
    public string Category { get; set; }

    [JsonPropertyName("thread-id")]
    public string ThreadId { get; set; }
}

internal class ApnAlert
{
    [JsonPropertyName("title")]
    public string Title { get; set; }

    [JsonPropertyName("body")]
    public string Body { get; set; }

    [JsonPropertyName("subtitle")]
    public string Subtitle { get; set; }

    [JsonPropertyName("title-loc-key")]
    public string TitleLocKey { get; set; }

    [JsonPropertyName("title-loc-args")]
    public string[] TitleLocArgs { get; set; }

    [JsonPropertyName("loc-key")]
    public string LocKey { get; set; }

    [JsonPropertyName("loc-args")]
    public string[] LocArgs { get; set; }

    [JsonPropertyName("action-loc-key")]
    public string ActionLocKey { get; set; }

    [JsonPropertyName("launch-image")]
    public string LaunchImage { get; set; }
}
