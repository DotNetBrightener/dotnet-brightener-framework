namespace DotNetBrightener.Identity.Constants;

/// <summary>
/// Default system roles
/// </summary>
public static class DefaultRoles
{
    /// <summary>
    ///     Administrator role with full system access
    /// </summary>
    public const string Administrator = "Administrator";

    /// <summary>
    ///     Account administrator role with full account access
    /// </summary>
    public const string AccountAdministrator = "AccountAdministrator";

    /// <summary>
    ///     User manager role with user management permissions
    /// </summary>
    public const string UserManager = "UserManager";

    /// <summary>
    ///     Standard user role with basic permissions
    /// </summary>
    public const string User = "User";

    /// <summary>
    ///     Guest user role with limited permissions
    /// </summary>
    public const string Guest = "Guest";

    /// <summary>
    ///     Anonymous user role for unauthenticated users
    /// </summary>
    public const string Anonymous = "Anonymous";

    /// <summary>
    ///     Gets all default role names
    /// </summary>
    public static readonly string[] All = 
    {
        Administrator,
        AccountAdministrator,
        UserManager,
        User,
        Guest,
        Anonymous
    };
}
