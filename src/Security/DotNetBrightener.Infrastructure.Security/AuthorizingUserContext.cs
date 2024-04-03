using System.Security.Claims;

namespace DotNetBrightener.Infrastructure.Security;

/// <summary>
/// Represents event which will be fired when the user is being authorized by the system
/// </summary>
public class AuthorizingUserContext
{
    /// <summary>
    /// The identifier of the user to authorize
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// Output claims which will be added to the current context
    /// </summary>
    public List<Claim> Claims { get; set; }

    /// <summary>
    /// Indicates that the user is authenticated or not
    /// </summary>
    public bool IsUserAuthenticated { get; set; }

    /// <summary>
    /// The current <see cref="ClaimsPrincipal"/> object that contains the current user's information
    /// </summary>
    public ClaimsPrincipal ContextUser { get; set; }
}