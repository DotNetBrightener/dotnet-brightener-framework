using DotNetBrightener.Infrastructure.Security;

// ReSharper disable once CheckNamespace
namespace System.Security.Claims;

public static class ClaimsPrincipalExtensions
{
    public static bool HasRole(this ClaimsPrincipal user, string roleCode)
    {
        var userRolesFromClaim = user?.FindAll(CommonUserClaimKeys.UserRole)
                                      .Select(_ => _.Value)
                                      .ToArray();

        if (userRolesFromClaim == null ||
            userRolesFromClaim.Length == 0)
        {
            return false;
        }

        var hasGivenRole = userRolesFromClaim.Any(_ => string.Equals(_,
                                                                     roleCode,
                                                                     StringComparison.OrdinalIgnoreCase));

        return hasGivenRole;
    }
}