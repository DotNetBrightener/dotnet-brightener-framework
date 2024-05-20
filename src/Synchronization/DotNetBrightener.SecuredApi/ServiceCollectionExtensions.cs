// ReSharper disable CheckNamespace

using DotNetBrightener.SecuredApi;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     Registers services that are required for the SecuredApi to the specified <see cref="IServiceCollection" />.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" />.</param>
    /// <returns>
    ///     Same instance of the <see cref="IServiceCollection" /> for chaining.
    /// </returns>
    public static IServiceCollection AddSecuredApi(this IServiceCollection services)
    {
        SecuredApiHandlerRouter handlerRouter = new();

        services.AddSingleton(handlerRouter);

        var builder = new SecureApiBuilder
        {
            HandlerRouter = handlerRouter,
            Services      = services
        };

        services.AddSingleton(builder);

        return services;
    }
}