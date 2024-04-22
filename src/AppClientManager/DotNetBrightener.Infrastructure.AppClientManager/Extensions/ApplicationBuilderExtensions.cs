using DotNetBrightener.Infrastructure.AppClientManager.Middlewares;
// ReSharper disable CheckNamespace

namespace Microsoft.AspNetCore.Builder;

public static class ApplicationBuilderExtensions
{
    /// <summary>
    ///     Register the CORS middleware that handles cross-origin requests from the managed app clients
    /// </summary>
    /// <param name="app"></param>
    /// <returns></returns>
    public static IApplicationBuilder UseAppClientCorsPolicy(this IApplicationBuilder app)
    {
        app.UseMiddleware<AppClientCorsEnableMiddleware>();

        return app;
    }
}