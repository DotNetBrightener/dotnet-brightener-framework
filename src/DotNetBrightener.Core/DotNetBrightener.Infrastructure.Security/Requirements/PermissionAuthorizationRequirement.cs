using System;
using DotNetBrightener.Infrastructure.Security.Permissions;
using Microsoft.AspNetCore.Authorization;

namespace DotNetBrightener.Infrastructure.Security.Requirements
{
    /// <summary>
    ///     Represents a requirement of authorizing a specified permission
    /// </summary>
    public class PermissionAuthorizationRequirement : IAuthorizationRequirement
    {
        /// <summary>
        ///     Initializes the permission requirement with the permission key
        /// </summary>
        /// <param name="permissionKey">Key of the permission</param>
        public PermissionAuthorizationRequirement(string permissionKey)
        {
            Permission = new Permission(permissionKey);
        }

        /// <summary>
        ///     Initialize the permission requirement with given <see cref="Permission"/> object
        /// </summary>
        /// <param name="permission"></param>
        public PermissionAuthorizationRequirement(Permission permission)
        {
            Permission = permission ?? throw new ArgumentNullException(nameof(permission));
        }

        public Permission Permission { get; set; }
    }
}