using DotNetBrightener.Infrastructure.Security.Requirements;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace DotNetBrightener.Infrastructure.Security.AuthorizationHandlers;

/// <summary>
///     Represent the provider that tells the system what roles to consider as System Administrators
/// </summary>
public interface ISysAdminRoleProvider
{
    string[] SysAdminRole { get; }
}

/// <summary>
///     Represents the <see cref="IAuthorizationHandler"/> that grants all permissions to the System Admin
/// </summary>
/// <param name="administratorProviders"></param>
public class SysAdminAuthorizer(IEnumerable<ISysAdminRoleProvider> administratorProviders) : IAuthorizationHandler
{
    public Task HandleAsync(AuthorizationHandlerContext context)
    {
        var sysAdminRoles = administratorProviders.SelectMany(x => x.SysAdminRole)
                                                  .ToArray();

        foreach (var sysAdminRole in sysAdminRoles)
        {
            if (context.User.HasRole(sysAdminRole) ||
                context.User.IsInRole(sysAdminRole))
            {
                GrantAllPermissions(context);

                break;
            }
        }

        return Task.CompletedTask;
    }

    private static void GrantAllPermissions(AuthorizationHandlerContext context)
    {
        foreach (var requirement in context.Requirements
                                           .OfType<PermissionsAuthorizationRequirement>())
        {
            context.Succeed(requirement);
        }
    }
}