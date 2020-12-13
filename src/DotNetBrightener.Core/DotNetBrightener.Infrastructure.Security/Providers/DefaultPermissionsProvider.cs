using DotNetBrightener.Core.Permissions;
using System.Collections.Generic;

namespace DotNetBrightener.Infrastructure.Security.Services
{
    public class DefaultPermissionsProvider : IPermissionProvider
    {
        public const string SystemPermissionGroupName = "System Permissions";

        public static readonly Permission ManagePermissions = new Permission("System.Permissions.ManagePermissions");

        public string PermissionGroupName => SystemPermissionGroupName;

        public IEnumerable<Permission> GetPermissions()
        {
            return new List<Permission>
            {
                ManagePermissions
            };
        }
    }
}