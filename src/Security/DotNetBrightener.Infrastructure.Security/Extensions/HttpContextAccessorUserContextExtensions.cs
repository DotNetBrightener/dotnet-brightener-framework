// ReSharper disable CheckNamespace
namespace Microsoft.AspNetCore.Http;

public static class HttpContextAccessorUserContextExtensions
{
    /// <summary>
    ///     Gets the Identifier of the current logged-in user from the request
    /// </summary>
    /// <param name="httpContextAccessor">The <see cref="IHttpContextAccessor" /> to access the current request information</param>
    /// <returns>The identifier of the user in <see cref="long"/>, if found. Otherwise, <c>null</c></returns>
    public static long? GetCurrentUserId(this IHttpContextAccessor httpContextAccessor)
        => httpContextAccessor.HttpContext?.GetCurrentUserId<long>();

    /// <summary>
    ///     Gets the Identifier of the current logged-in user from the request
    /// </summary>
    /// <typeparam name="T">The type of the user identifier</typeparam>
    /// <param name="httpContextAccessor">The <see cref="IHttpContextAccessor" /> to access the current request information</param>
    /// <returns>The identifier of the user in <typeparamref name="T"/>, if found. Otherwise, <c>null</c></returns>
    public static T GetCurrentUserId<T>(this IHttpContextAccessor httpContextAccessor)
    {
        return httpContextAccessor.HttpContext is null
                   ? default
                   : httpContextAccessor.HttpContext.GetCurrentUserId<T>();
    }

    /// <summary>
    ///     Gets the username of the current logged-in user from the request
    /// </summary>
    /// <param name="httpContextAccessor">The <see cref="IHttpContextAccessor" /> to access the current request information</param>
    /// <returns>The username of the user if found. Otherwise, <c>null</c></returns>
    public static string GetCurrentUserName(this IHttpContextAccessor httpContextAccessor)
        => httpContextAccessor.HttpContext?.GetCurrentUserName();

    /// <summary>
    ///     Gets the Identifier of the current logged-in user from the request
    /// </summary>
    /// <param name="httpContext">The current request information</param>
    /// <returns>The identifier of the user in <see cref="long"/>, if found. Otherwise, <c>null</c></returns>
    public static long? GetCurrentUserId(this HttpContext httpContext)
    {
        return GetCurrentUserId<long>(httpContext);
    }

    /// <summary>
    ///     Gets the Identifier of the current logged-in user from the request
    /// </summary>
    /// <param name="httpContext">The current request information</param>
    /// <returns>The identifier of the user in <see cref="long"/>, if found. Otherwise, <c>null</c></returns>
    public static T GetCurrentUserId<T>(this HttpContext httpContext)
    {
        var userContext = httpContext?.User;

        if (userContext?.Identity?.IsAuthenticated != true)
        {
            return default(T);
        }

        var userIdClaim = userContext.FindFirst("sub") ?? userContext.FindFirst("USER_ID");

        if (userIdClaim != null)
        {
            var tValue = Convert.ChangeType(userIdClaim.Value, typeof(T));

            if (tValue is T result)
                return result;
        }

        return default(T);
    }

    /// <summary>
    ///     Gets the username of the current logged-in user from the request
    /// </summary>
    /// <param name="httpContext">The current request information</param>
    /// <returns>The username of the user if found. Otherwise, <c>null</c></returns>
    public static string GetCurrentUserName(this HttpContext httpContext)
    {
        var userContext = httpContext?.User;

        if (userContext?.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        return userContext.FindFirst("email")?.Value ??
               userContext.FindFirst("USERNAME")?.Value;
    }
}