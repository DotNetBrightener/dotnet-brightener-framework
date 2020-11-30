using System;
using DotNetBrightener.Core.Permissions;
using Microsoft.AspNetCore.Authorization;

namespace DotNetBrightener.Core.Security.GenericPermissionAuthorizationHandler
{
    public class PermissionRequirement : IAuthorizationRequirement
    {
        public PermissionRequirement(Permission permission)
        {
            Permission = permission ?? throw new ArgumentNullException(nameof(permission));
        }

        public PermissionRequirement(string permissionKey)
        {
            Permission = new Permission(permissionKey);
        }

        public Permission Permission { get; set; }
    }
}