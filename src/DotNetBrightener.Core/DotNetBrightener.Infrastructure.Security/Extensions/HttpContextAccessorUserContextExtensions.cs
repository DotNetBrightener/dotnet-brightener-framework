using Microsoft.AspNetCore.Http;

namespace DotNetBrightener.Infrastructure.Security.Extensions
{
    public static class HttpContextAccessorUserContextExtensions
    {
        /// <summary>
        ///     Gets the Identifier of the current logged in user from the request
        /// </summary>
        /// <param name="httpContextAccessor">The <see cref="IHttpContextAccessor" /> to access the current request information</param>
        /// <returns>The identifier of the user in <see cref="long"/>, if found. Otherwise, <c>null</c></returns>
        public static long? GetCurrentUserId(this IHttpContextAccessor httpContextAccessor)
        {
            return GetCurrentUserId(httpContextAccessor.HttpContext);
        }

        /// <summary>
        ///     Gets the username of the current logged in user from the request
        /// </summary>
        /// <param name="httpContextAccessor">The <see cref="IHttpContextAccessor" /> to access the current request information</param>
        /// <returns>The username of the user if found. Otherwise, <c>null</c></returns>
        public static string GetCurrentUserName(this IHttpContextAccessor httpContextAccessor)
        {
            return GetCurrentUserName(httpContextAccessor.HttpContext);
        }

        /// <summary>
        ///     Gets the Identifier of the current logged in user from the request
        /// </summary>
        /// <param name="httpContext">The current request information</param>
        /// <returns>The identifier of the user in <see cref="long"/>, if found. Otherwise, <c>null</c></returns>
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

        /// <summary>
        ///     Gets the username of the current logged in user from the request
        /// </summary>
        /// <param name="httpContext">The current request information</param>
        /// <returns>The username of the user if found. Otherwise, <c>null</c></returns>
        public static string GetCurrentUserName(this HttpContext httpContext)
        {
            var userContext = httpContext.User;

            if (!userContext.Identity.IsAuthenticated)
            {
                return null;
            }

            return userContext.FindFirst(CommonUserClaimKeys.UserName)?.Value;
        }
    }
}
