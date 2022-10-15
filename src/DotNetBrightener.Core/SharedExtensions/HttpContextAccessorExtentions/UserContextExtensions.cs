namespace Microsoft.AspNetCore.Http;

internal static class UserContextExtensions
{
    public static long? GetCurrentUserId(this IHttpContextAccessor httpContextAccessor)
    {
        return GetCurrentUserId(httpContextAccessor.HttpContext);
    }

    public static long? GetCurrentUserId(this HttpContext httpContext)
    {
        var userContext = httpContext.User;

        if (!userContext.Identity.IsAuthenticated)
        {
            return null;
        }

        var userIdClaim = userContext.FindFirst(CommonUserClaimKeys.UserId);

        if (userIdClaim != null)
        {
            long.TryParse(userIdClaim.Value, out var userId);

            return userId;
        }

        return null;
    }
}