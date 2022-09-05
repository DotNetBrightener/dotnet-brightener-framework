// DotNetBrightenerFramework - DotNetBrightener.Infrastructure.Security

using DotNetBrightener.Infrastructure.Security.Services;

namespace DotNetBrightener.Infrastructure.Security.Providers;

public class DefaultPermissions: IPermissionsDeclaration
{
    /// <summary>
    ///     Granted permissions management rights
    /// </summary>
    public static string ManagePermissions => "System.Permissions.ManagePermissions";
}