using DotNetBrightener.Infrastructure.Security.Permissions;

namespace DotNetBrightener.Infrastructure.Security.Services;

public class PermissionsContainer : IPermissionsContainer
{
    private readonly IEnumerable<IPermissionProvider> _permissionProviders;

    private Dictionary<string, Permission> _availablePermissions;

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
            throw new InvalidOperationException(
                                                $"There are permissions with same keys {string.Join(", ", existingPermissionKeys)}. " +
                                                $"Please fix the issue to get the application launch properly.");
        }
    }

    public IEnumerable<Permission> GetAvailablePermissions()
    {
        if (_availablePermissions == null || !_availablePermissions.Any())
            LoadAndValidatePermissions();

        return _availablePermissions!.Values;
    }

    public Permission GetPermissionByKey(string permissionKey)
    {
        if (_availablePermissions == null || !_availablePermissions.Any())
            LoadAndValidatePermissions();

        if (_availablePermissions!.TryGetValue(permissionKey, out var permission))
            return permission;

        return null;
    }
}