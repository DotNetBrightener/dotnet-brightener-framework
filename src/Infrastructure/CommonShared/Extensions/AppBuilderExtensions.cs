using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using WebApp.CommonShared.Endpoints;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class AppBuilderExtensions
{
    /// <summary>
    ///     Adds the required middlewares for the web application to the specified <see cref="IApplicationBuilder"/>
    /// </summary>
    /// <remarks>
    ///     The pre-configured middlewares are<br />
    ///     - ForwardedHeaders<br />
    ///     - HttpsRedirection<br />
    ///     - ExceptionHandler<br />
    ///
    ///     This method should be called before all other middlewares in the pipeline
    /// </remarks>
    /// <param name="app">The <see cref="IApplicationBuilder"/></param>
    /// <returns>
    ///     The same instance of this <see cref="IApplicationBuilder"/> for chaining operations
    /// </returns>
    public static IApplicationBuilder UseCommonWebAppServices(this IApplicationBuilder app)
    {
        app.UseForwardedHeaders();

        if (Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") != "true")
        {
            app.UseHttpsRedirection();
        }

        var allowedOriginConfigs = Environment.GetEnvironmentVariable("ASPNETCORE__AllowedCorsOrigins");

        if (!string.IsNullOrEmpty(allowedOriginConfigs))
        {
            app.UseCors("Default__AllowedOrigins");
        }

        return app;
    }
}