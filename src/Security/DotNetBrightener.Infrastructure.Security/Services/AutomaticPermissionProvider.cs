using System.Collections.Generic;
using DotNetBrightener.Infrastructure.Security.Extensions;
using DotNetBrightener.Infrastructure.Security.Permissions;

namespace DotNetBrightener.Infrastructure.Security.Services;

public abstract class AutomaticPermissionProvider : IPermissionsDeclaration, IPermissionProvider
{
    public string PermissionGroupName => this.GetType().FullName;

    public IEnumerable<Permission> GetPermissions()
    {
        var permissions = GetType().ExtractConstantsPermissions();

        return permissions;
    }
}