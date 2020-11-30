using System.Linq;
using System.Security.Claims;

internal static class ClaimsPrincipalExtensions
{
    public static long[] RetrieveRoleIds(this ClaimsPrincipal claimsPrincipal)
    {
        if (!claimsPrincipal.Identity.IsAuthenticated)
            return null;

        var userRoles = claimsPrincipal.FindAll(CommonUserClaimKeys.UserRoleId)
                                       .Select(_ => long.TryParse(_.Value, out var roleId) ? roleId : -1000)
                                       .Where(_ => _ != -1000);

        return userRoles.ToArray();
    }
}