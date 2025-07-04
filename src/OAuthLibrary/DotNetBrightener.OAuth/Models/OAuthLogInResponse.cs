using System.Net;

namespace DotNetBrightener.OAuth.Models;

/// <summary>
/// 
/// </summary>
public class OAuthLogInResponse
{
    /// <summary>
    /// 
    /// </summary>
    public HttpStatusCode StatusCode { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public string Status { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public OAuthUser UserInformation { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public bool IsLoginOnly { get; set; }
}