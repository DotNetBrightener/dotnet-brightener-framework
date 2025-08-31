using ActivityLog.Models;
using DotNetBrightener.DataAccess;
using Microsoft.AspNetCore.Http;

namespace ActivityLog.Services;

/// <summary>
/// Provides context information for activity logging
/// </summary>
public interface IActivityLogContextProvider
{
    /// <summary>
    /// Gets the current correlation ID for tracking related activities
    /// </summary>
    /// <returns>The correlation ID or null if not available</returns>
    Guid? GetCorrelationId();

    /// <summary>
    /// Gets the current user context
    /// </summary>
    /// <returns>The user context or null if not available</returns>
    UserContext? GetUserContext();

    /// <summary>
    /// Gets the current HTTP context information
    /// </summary>
    /// <returns>The HTTP context information or null if not available</returns>
    HttpContextInfo? GetHttpContext();

    /// <summary>
    /// Sets the correlation ID for the current context
    /// </summary>
    /// <param name="correlationId">The correlation ID to set</param>
    void SetCorrelationId(Guid correlationId);
}

/// <summary>
/// Default implementation of IActivityLogContextProvider
/// </summary>
public class ActivityLogContextProvider(
    IHttpContextAccessor?         httpContextAccessor = null,
    ICurrentLoggedInUserResolver? userResolver        = null)
    : IActivityLogContextProvider
{
    private static readonly AsyncLocal<Guid?> _correlationId = new();

    public Guid? GetCorrelationId()
    {
        // Try to get from AsyncLocal first
        if (_correlationId.Value.HasValue)
            return _correlationId.Value;

        // Try to get from HTTP context
        var httpContext = httpContextAccessor?.HttpContext;

        if (httpContext?.Items.TryGetValue("CorrelationId", out var correlationIdObj) == true &&
            correlationIdObj is Guid correlationId)
        {
            return correlationId;
        }

        // Generate new correlation ID
        var newCorrelationId = Guid.CreateVersion7();
        SetCorrelationId(newCorrelationId);

        return newCorrelationId;
    }

    public UserContext? GetUserContext()
    {
        if (userResolver == null)
            return null;

        try
        {
            var userContext = new UserContext
            {
                UserName = userResolver.CurrentUserName
            };

            // Try to get user ID if available
            if (long.TryParse(userResolver.CurrentUserId?.ToString(), out var userId))
            {
                userContext.UserId = userId;
            }

            // Add claims from HTTP context if available
            var httpContext = httpContextAccessor?.HttpContext;

            if (httpContext?.User?.Identity?.IsAuthenticated == true)
            {
                foreach (var claim in httpContext.User.Claims)
                {
                    userContext.Claims[claim.Type] = claim.Value;
                }
            }

            return userContext;
        }
        catch (Exception)
        {
            // Return null if user context cannot be resolved
            return null;
        }
    }

    public HttpContextInfo? GetHttpContext()
    {
        var httpContext = httpContextAccessor?.HttpContext;

        if (httpContext == null)
            return null;

        try
        {
            var request = httpContext.Request;
            var httpContextInfo = new HttpContextInfo
            {
                Method    = request.Method,
                Url       = $"{request.Scheme}://{request.Host}{request.Path}{request.QueryString}",
                UserAgent = request.Headers.UserAgent.ToString(),
                IpAddress = GetClientIpAddress(httpContext)
            };

            // Add important headers
            foreach (var header in request.Headers.Where(h => IsImportantHeader(h.Key)))
            {
                httpContextInfo.Headers[header.Key] = string.Join(", ", header.Value.ToString());
            }

            return httpContextInfo;
        }
        catch (Exception)
        {
            // Return null if HTTP context cannot be processed
            return null;
        }
    }

    public void SetCorrelationId(Guid correlationId)
    {
        _correlationId.Value = correlationId;

        // Also set in HTTP context if available
        var httpContext = httpContextAccessor?.HttpContext;

        if (httpContext != null)
        {
            httpContext.Items["CorrelationId"] = correlationId;
        }
    }

    private static string? GetClientIpAddress(HttpContext httpContext)
    {
        // Check for forwarded IP first
        var forwardedFor = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();

        if (!string.IsNullOrEmpty(forwardedFor))
        {
            return forwardedFor.Split(',')[0].Trim();
        }

        // Check for real IP
        var realIp = httpContext.Request.Headers["X-Real-IP"].FirstOrDefault();

        if (!string.IsNullOrEmpty(realIp))
        {
            return realIp;
        }

        // Fall back to connection remote IP
        return httpContext.Connection.RemoteIpAddress?.ToString();
    }

    private static bool IsImportantHeader(string headerName)
    {
        var importantHeaders = new[]
        {
            "Authorization",
            "Content-Type",
            "Accept",
            "Accept-Language",
            "Accept-Encoding",
            "Cache-Control",
            "Connection",
            "Host",
            "Referer",
            "X-Requested-With",
            "X-Forwarded-For",
            "X-Real-IP"
        };

        return importantHeaders.Contains(headerName, StringComparer.OrdinalIgnoreCase);
    }
}