using System;
using System.Collections.Generic;
using System.Linq;
using DotNetBrightener.Core.Permissions;

namespace DotNetBrightener.Infrastructure.Security.Services
{
    public interface IPermissionsContainer
    {
        /// <summary>
        ///     Load all permissions from modules and validate for any duplicated permission key
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if permission key gets duplicated</exception>
        void LoadAndValidatePermissions();

        /// <summary>
        ///     Get all available permissions in the system
        /// </summary>
        /// <returns></returns>
        IEnumerable<Permission> GetAvailablePermissions();

        /// <summary>
        ///     Gets permission object by its key
        /// </summary>
        /// <param name="permissionKey">Key of the <see cref="Permission"/></param>
        /// <returns></returns>
        Permission GetPermissionByKey(string permissionKey);
    }

    public class PermissionsContainer : IPermissionsContainer
    {
        private readonly IEnumerable<IPermissionProvider> _permissionProviders;

        private Dictionary<string, Permission> _availablePermissions = null;

        public PermissionsContainer(IEnumerable<IPermissionProvider> permissionProviders)
        {
            _permissionProviders = permissionProviders;
        }

        public void LoadAndValidatePermissions()
        {
            _availablePermissions = new Dictionary<string, Permission>();
            var existingPermissionKeys = new List<string>();

            foreach (var permissionProvider in _permissionProviders)
            {
                var modulePermissions = permissionProvider.GetPermissions().ToArray();

                foreach (var modulePermission in modulePermissions)
                {
                    if (_availablePermissions.ContainsKey(modulePermission.PermissionKey))
                    {
                        existingPermissionKeys.Add(modulePermission.PermissionKey);
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(modulePermission.PermissionGroup))
                        {
                            modulePermission.PermissionGroup = permissionProvider.PermissionGroupName;
                        }
                        _availablePermissions.Add(modulePermission.PermissionKey, modulePermission);
                    }
                }
            }

            if (existingPermissionKeys.Any())
            {
                throw new InvalidOperationException($"There are permissions with same keys {string.Join(", ", existingPermissionKeys)}. Please fix the issue to get the application launch properly.");
            }
        }

        public IEnumerable<Permission> GetAvailablePermissions()
        {
            if (_availablePermissions == null || !_availablePermissions.Any())
                LoadAndValidatePermissions();

            return _availablePermissions.Values;
        }

        public Permission GetPermissionByKey(string permissionKey)
        {
            if (_availablePermissions == null || !_availablePermissions.Any())
                LoadAndValidatePermissions();

            if (_availablePermissions.TryGetValue(permissionKey, out var permission))
                return permission;

            return null;
        }
    }
}