namespace DotNetBrightener.OAuth.Models;

/// <summary>
///     Represents the information of the user retrieved from the External OAuth system
/// </summary>
public class OAuthUser
{
    /// <summary>
    /// 
    /// </summary>
    public string Email { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public string FirstName { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public string MiddleName { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public string LastName { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public string ExternalKey { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public string AccessToken { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public string ProfileImageUrl { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public string ProfileImageUrlCropped { get; set; }
}