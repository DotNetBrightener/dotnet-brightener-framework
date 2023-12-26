using System;
using Newtonsoft.Json;

namespace DotNetBrightener.Infrastructure.ApiKeyAuthentication.Models;

public class ApiKey
{
    public string Name { get; set; }
    
    public string ApiTokenId { get; set; }

    [JsonIgnore]
    public string ApiKeyHashedToken { get; set; }

    [JsonIgnore]
    public string SaltValue { get; set; }

    public DateTime? ExpiresAtUtc { get; set; }

    public string[] Scopes { get; set; }
}