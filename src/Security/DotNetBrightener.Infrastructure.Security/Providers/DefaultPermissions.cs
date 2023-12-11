using DotNetBrightener.Infrastructure.Security.Services;

namespace DotNetBrightener.Infrastructure.Security.Providers;

public class DefaultPermissions: AutomaticPermissionProvider
{
    /// <summary>
    ///     Granted permissions management rights
    /// </summary>
    public const string ManagePermissions = "System.Permissions.ManagePermissions";

    /// <summary>
    ///     Permission to impersonate other users
    /// </summary>
    public const string Impersonation = "System.Permissions.ImpersonateAsOtherUser";
}