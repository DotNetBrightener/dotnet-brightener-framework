using System.Collections.Generic;

namespace DotNetBrightener.Core.Permissions
{
    public class SystemPermissions
    {
        public const string ManagePermissions = "System.Permissions.ManagePermissions";
        public const string Impersonation = "System.Permissions.Impersonation";
        public const string CancelImpersonation = "System.Permissions.CancelImpersonation";
    }

    public class DefaultPermissionsProvider : IPermissionProvider
    {
        public string PermissionGroupName => "System Permissions";

        public IEnumerable<Permission> GetPermissions()
        {
            return new List<Permission>
            {
                SystemPermissions.ManagePermissions,
                SystemPermissions.Impersonation
            };
        }
    }
}