namespace DotNetBrightener.OAuth.Models;

/// <summary>
/// Represent OAuth Request 
/// </summary>
public class OAuthRequestModel
{
    public OAuthRequestModel()
    {
        ExtraParameters = new Dictionary<string, object>();
    }

    /// <summary>
    /// 
    /// </summary>
    public string RefererUrl { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public string RedirectUrl { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public string RequestId { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public string CallbackUrl { get; set; }

    public long? LinkedUserId { get; set; }

    public bool ValidateUserOnly { get; set; }

    public Dictionary<string, object> ExtraParameters { get; set; }
}