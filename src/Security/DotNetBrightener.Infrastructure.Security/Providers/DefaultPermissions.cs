using DotNetBrightener.Infrastructure.Security.Services;

namespace DotNetBrightener.Infrastructure.Security.Providers;

public class DefaultPermissions: AutomaticPermissionProvider
{
    /// <summary>
    ///     Granted permissions management rights
    /// </summary>
    public const string ManagePermissions = "System.Permissions.ManagePermissions";
}