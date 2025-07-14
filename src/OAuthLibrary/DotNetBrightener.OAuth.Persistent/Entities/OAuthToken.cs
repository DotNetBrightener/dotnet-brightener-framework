using DotNetBrightener.DataAccess.Models;

namespace DotNetBrightener.OAuth.Persistent.Entities;

public class OAuthToken : BaseEntityWithAuditInfo
{
    public string Provider { get; set; }

    public string ExternalUserId { get; set; }

    public string AccessToken { get; set; }

    public string RefreshToken { get; set; }

    public string TokenScopes { get; set; }

    public DateTimeOffset? TokenExpiry { get; set; }
}