using DotNetBrightener.Infrastructure.Security.Services;

namespace DotNetBrightener.MultiTenancy.Permissions;

public class TenantManagementPermissions : AutomaticPermissionProvider
{
    /// <summary>
    ///     Provides permission to manage tenants
    /// </summary>
    public const string ManageTenants = "MultiTenant.Permissions.ManageTenants";

    /// <summary>
    ///     Permission to access resources from all tenants
    /// </summary>
    public const string AccessAllTenantsResources = "MultiTenant.Permissions.AllTenantsResourceAccess";
}