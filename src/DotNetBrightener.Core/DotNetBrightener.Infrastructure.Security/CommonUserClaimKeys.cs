using System.Security.Claims;

namespace DotNetBrightener.Infrastructure.Security;

/// <summary>
///     The default claim keys for the user authorization 
/// </summary>
public class CommonUserClaimKeys
{
    /// <summary>
    ///     The identifier of the <see cref="Claim"/> that represents the username
    /// </summary>
    public const string UserName = "USERNAME";

    /// <summary>
    ///     The identifier of the <see cref="Claim"/> that represents the display name of the user
    /// </summary>
    public const string UserFullName = "USER_FULLNAME";

    /// <summary>
    ///     The identifier of the <see cref="Claim"/> that represents the identifier of the user
    /// </summary>
    public const string UserId = "USER_ID";

    /// <summary>
    ///     The identifier of the <see cref="Claim"/> that represents the user's role
    /// </summary>
    public const string UserRole = "USER_ROLE";

    /// <summary>
    ///     The identifier of the <see cref="Claim"/> that represents the identifier of the user's role
    /// </summary>
    public const string UserRoleId = "USER_ROLE_ID";

    /// <summary>
    ///     The identifier of the <see cref="Claim"/> that represents a permission that the user is granted
    /// </summary>
    public const string UserPermission = "USER_PERMISSION";

    /// <summary>
    ///     The identifier of the <see cref="Claim"/> that represents the identifier of the log in session
    /// </summary>
    public const string SessionId = "SESSION_ID";
}