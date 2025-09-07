using System.Security.Claims;

namespace DotNetBrightener.Identity.Constants;

/// <summary>
/// The default claim types for the identity system
/// </summary>
public static class IdentityClaimTypes
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
    ///     The identifier of the <see cref="Claim"/> that represents the identifier of the log in session
    /// </summary>
    public const string SessionId = "SESSION_ID";

    /// <summary>
    ///     The identifier of the <see cref="Claim"/> that represents the current account context
    /// </summary>
    public const string CurrentAccountId = "CURRENT_ACCOUNT_ID";

    /// <summary>
    ///     The identifier of the <see cref="Claim"/> that represents the current account name
    /// </summary>
    public const string CurrentAccountName = "CURRENT_ACCOUNT_NAME";

    /// <summary>
    ///     The identifier of the <see cref="Claim"/> that represents a permission
    /// </summary>
    public const string Permission = "Permission";

    /// <summary>
    ///     The identifier of the <see cref="Claim"/> that represents account membership
    /// </summary>
    public const string AccountMembership = "ACCOUNT_MEMBERSHIP";

    /// <summary>
    ///     The identifier of the <see cref="Claim"/> that represents whether user can access sub-accounts
    /// </summary>
    public const string CanAccessSubAccounts = "CAN_ACCESS_SUB_ACCOUNTS";
}
