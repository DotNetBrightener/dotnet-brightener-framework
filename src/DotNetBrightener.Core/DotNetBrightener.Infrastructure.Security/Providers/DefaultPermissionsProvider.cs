using System.Collections.Generic;
using DotNetBrightener.Infrastructure.Security.Extensions;
using DotNetBrightener.Infrastructure.Security.Permissions;
using DotNetBrightener.Infrastructure.Security.Services;

namespace DotNetBrightener.Infrastructure.Security.Providers;

public class DefaultPermissionsProvider : IPermissionProvider
{
    public const string SystemPermissionGroupName = "System Permissions";
    
    public string PermissionGroupName => SystemPermissionGroupName;

    public IEnumerable<Permission> GetPermissions()
    {
        return typeof(DefaultPermissions).ExtractConstantsPermissions();
    }
}