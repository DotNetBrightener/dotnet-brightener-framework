using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using System.Text;

namespace DotNetBrightener.SiteSettings.Internal;

/// <summary>
///     The filter called before the action execution, to read the request body into a string.
///     In the action, obtain the body by calling the <see cref="IHttpContextAccessor.ObtainRequestBody"/> method.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
internal class SettingJsonBodyReader
    : Attribute,
      IAuthorizationFilter,
      IAsyncAuthorizationFilter
{
    internal const string RequestBodyKey = "settings_request_body";

    private static readonly string[] SupportedMethods =
    [
        "POST", "PATCH", "PUT"
    ];

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        if (!SupportedMethods.Any(method => method.Equals(context.HttpContext.Request.Method,
                                                          StringComparison.OrdinalIgnoreCase)))
            return;

        var req = context.HttpContext.Request;
        
        var syncIoFeature = context.HttpContext.Features.Get<IHttpBodyControlFeature>();

        if (syncIoFeature == null)
            return;

        syncIoFeature.AllowSynchronousIO = true;

        req.EnableBuffering();

        if (!req.Body.CanSeek)
            return;

        req.Body.Seek(0, SeekOrigin.Begin);

        var httpContextAccessor = context.HttpContext.RequestServices.GetService<IHttpContextAccessor>();

        using (var reader = new StreamReader(
                                             req.Body,
                                             encoding: Encoding.UTF8,
                                             detectEncodingFromByteOrderMarks: false,
                                             bufferSize: 8192,
                                             leaveOpen: true))
        {
            var bodyStreamString = reader.ReadToEnd();

            context.HttpContext.Items[RequestBodyKey] = bodyStreamString;
        }

        // rewind the body back to beginning, so it can be processed by other filters.
        req.Body.Seek(0, SeekOrigin.Begin);
    }

    public Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        return Task.Run(() =>
        {
            OnAuthorization(context);
        });
    }
}