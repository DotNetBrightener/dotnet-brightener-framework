
using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace DotNetBrightener.OAuth.Integration.Apple.Models;

public class AuthorizationToken
{
    [JsonPropertyName("access_token")]
    [JsonProperty("access_token")]
    public string AuthorizationCode { get; set; }

    [JsonPropertyName("expires_in")]
    [JsonProperty("expires_in")]
    public int ExpiresInSeconds { get; set; }

    [JsonPropertyName("id_token")]
    [JsonProperty("id_token")]
    public string Token { get; set; }

    [JsonPropertyName("refresh_token")]
    [JsonProperty("refresh_token")]
    public string RefreshToken { get; set; }

    [JsonPropertyName("token_type")]
    [JsonProperty("token_type")]
    public string TokenType { get; set; }

    public AppleUserInformation UserInformation { get; set; }
}