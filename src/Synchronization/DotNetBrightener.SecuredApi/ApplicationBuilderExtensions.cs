using DotNetBrightener.SecuredApi;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable once CheckNamespace
namespace Microsoft.AspNetCore.Builder;

public static class ApplicationBuilderExtensions
{
    /// <summary>
    ///     Adds the middleware to handle secured API requests.
    /// </summary>
    /// <param name="app"></param>
    /// <param name="requestPath"></param>
    /// <returns></returns>
    public static IApplicationBuilder UseSecureApiHandle(this IApplicationBuilder app,
                                                         string                   requestPath = null)
    {
        var metadata = app.ApplicationServices
                          .GetRequiredService<SecuredApiHandlerRouter>();

        app.UseMiddleware<SecureApiHandleMiddleware>(new SecuredApiOptions
        {
            BasePath = !string.IsNullOrEmpty(requestPath) ? "/" + requestPath.Trim().TrimStart('/') : ""
        });

        metadata.MiddlewareRegistered = true;

        return app;
    }

    /// <summary>
    ///     Maps the given secure API handler to the specified action name.
    /// </summary>
    /// <typeparam name="TSecureApiHandler">
    ///     The type of the secured API handler
    /// </typeparam>
    /// <param name="app"></param>
    /// <param name="actionName"></param>
    /// <returns></returns>
    internal static IEndpointRouteBuilder MapSecuredApiHandle<TSecureApiHandler>(this IEndpointRouteBuilder app,
                                                                                 string actionName,
                                                                                 HttpMethod httpMethod = null)
        where TSecureApiHandler : BaseApiHandler
    {
        httpMethod ??= HttpMethod.Post;

        var metadata = app.ServiceProvider
                          .GetRequiredService<SecuredApiHandlerRouter>();

        if (!metadata.MiddlewareRegistered)
        {
            throw new
                InvalidOperationException($"{nameof(UseSecureApiHandle)}() method was not called. Please call it before mapping secured API handlers");
        }

        if (!metadata.TryGetValue(actionName, out var apiHandlerMetadata))
        {
            apiHandlerMetadata = new SecuredApiHandlerRoutingMetadata
            {
                RoutePattern = actionName.Trim().TrimStart('/'),
                HandlerType  = typeof(TSecureApiHandler),
                HttpMethod   = httpMethod
            };

            metadata.TryAdd(actionName, apiHandlerMetadata);
        }

        else
            apiHandlerMetadata.HandlerType = typeof(TSecureApiHandler);

        return app;
    }


    public static IEndpointRouteBuilder MapSecuredGet<TSecureApiHandler>(this IEndpointRouteBuilder endpointsBuilder,
                                                                         string                     pattern)
        where TSecureApiHandler : BaseApiHandler
    {
        return MapSecuredApiHandle<TSecureApiHandler>(endpointsBuilder, pattern, HttpMethod.Get);
    }

    public static IEndpointRouteBuilder MapSecuredPost<TSecureApiHandler>(this IEndpointRouteBuilder endpointsBuilder,
                                                                          string                     pattern)
        where TSecureApiHandler : BaseApiHandler
    {
        return MapSecuredApiHandle<TSecureApiHandler>(endpointsBuilder, pattern, HttpMethod.Post);
    }

    public static IEndpointRouteBuilder MapSecuredPut<TSecureApiHandler>(this IEndpointRouteBuilder endpointsBuilder,
                                                                         string                     pattern)
        where TSecureApiHandler : BaseApiHandler
    {
        return MapSecuredApiHandle<TSecureApiHandler>(endpointsBuilder, pattern, HttpMethod.Put);
    }

    public static IEndpointRouteBuilder MapSecuredDelete<TSecureApiHandler>(this IEndpointRouteBuilder endpointsBuilder,
                                                                            string                     pattern)
        where TSecureApiHandler : BaseApiHandler
    {
        return MapSecuredApiHandle<TSecureApiHandler>(endpointsBuilder, pattern, HttpMethod.Delete);
    }
}