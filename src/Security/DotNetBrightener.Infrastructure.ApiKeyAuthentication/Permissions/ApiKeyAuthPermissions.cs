using DotNetBrightener.Infrastructure.Security.Services;

namespace DotNetBrightener.Infrastructure.ApiKeyAuthentication.Permissions;

public class ApiKeyAuthPermissions : AutomaticPermissionProvider
{
    /// <summary>
    ///     Provides permission to manage API Keys
    /// </summary>
    public const string ManageApiKeys = "ApiKeyAuth.Permissions.ManageApiKeys";
}