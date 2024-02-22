using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetBrightener.WebApi.GenericCRUD.ActionFilters;

/// <summary>
///     The filter called before the action execution, to read the request body into a string.
///     In the action, obtain the body by calling the <see cref="IHttpContextAccessor.ObtainRequestBody"/> method.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequestBodyReader
    : Attribute,
      IAuthorizationFilter,
      IAsyncAuthorizationFilter
{
    private const string RequestBodyKey = "request_body";

    private static readonly string[] SupportedMethods =
    {
        "POST", "PATCH", "PUT"
    };

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        // if not POST / PATCH request, ignore
        if (!SupportedMethods.Any(method => method.Equals(context?.HttpContext.Request.Method,
                                                          StringComparison.OrdinalIgnoreCase)))
            return;

        var req = context.HttpContext.Request;

        // no point reading body if it's not available
        if (req.Body is null)
            return;

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

            httpContextAccessor.StoreValue(RequestBodyKey, bodyStreamString);
        }

        // rewind the body back to beginning so it can be processed by other filters.
        req.Body.Seek(0, SeekOrigin.Begin);
    }

    public Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        return Task.Run(() => OnAuthorization(context));
    }

    internal static string ObtainBody(IHttpContextAccessor httpContextAccessor)
    {
        return httpContextAccessor.RetrieveValue<string>(RequestBodyKey);
    }

    internal static TModel ObtainBodyAs<TModel>(IHttpContextAccessor httpContextAccessor)
    {
        var bodyObject = httpContextAccessor.RetrieveValue<TModel>();

        if (bodyObject is not null)
            return bodyObject;

        var bodyString = httpContextAccessor.RetrieveValue<string>(RequestBodyKey);

        bodyObject = JsonConvert.DeserializeObject<TModel>(bodyString);

        httpContextAccessor.StoreValue(bodyObject);

        return bodyObject;
    }

    internal static JObject ObtainBodyAsJObject(IHttpContextAccessor httpContextAccessor)
    {
        var bodyString = httpContextAccessor.RetrieveValue<string>(RequestBodyKey);

        return JObject.Parse(bodyString);
    }
}