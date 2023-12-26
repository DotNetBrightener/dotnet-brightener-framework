namespace DotNetBrightener.Infrastructure.ApiKeyAuthentication.Models;

public class GenerateApiKeyRequest
{
    public string Name { get; set; }

    public int? ExpiresInDays { get; set; }

    public string[] Scopes { get; set; }
}