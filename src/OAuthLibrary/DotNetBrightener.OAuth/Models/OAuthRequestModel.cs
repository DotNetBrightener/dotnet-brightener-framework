namespace DotNetBrightener.OAuth.Models;

public class OAuthRequestModel
{
    public string RefererUrl { get; set; }

    public string RedirectUrl { get; set; }

    public string RequestId { get; set; }

    public string CallbackUrl { get; set; }

    public long? LinkedUserId { get; set; }

    public bool ValidateUserOnly { get; set; }

    public Dictionary<string, object> ExtraParameters { get; set; } = new();

    public string[] Scopes { get; set; } = [];
}