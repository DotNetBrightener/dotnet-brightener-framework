using System.Security.Claims;

namespace DotNetBrightener.WebSocketExt.Services;

/// <summary>
///     Represents the service that generates the long-lived auth token for the authenticated user.
/// </summary>
public interface IWebSocketAuthTokenGenerator
{
    /// <summary>
    ///     Generates the long-lived auth token for the authenticated user
    /// </summary>
    /// <param name="userPrincipal">
    ///     The user's principal that has been authenticated
    /// </param>
    /// <param name="expiresIn">
    ///     The time span for which the token should be valid after generated
    /// </param>
    /// <returns>
    ///     The new long-lived auth token
    /// </returns>
    Task<WebSocketAuthenticationResult> GenerateToken(ClaimsPrincipal userPrincipal, TimeSpan expiresIn);
}

public class WebSocketAuthenticationResult
{
    public string AuthenticationToken { get; set; }

    public DateTimeOffset ExpiresAt { get; set; }
}