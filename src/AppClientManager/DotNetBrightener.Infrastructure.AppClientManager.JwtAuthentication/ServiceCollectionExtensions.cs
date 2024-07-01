using DotNetBrightener.Infrastructure.AppClientManager;
using DotNetBrightener.Infrastructure.AppClientManager.JwtAuthentication;
using DotNetBrightener.Infrastructure.JwtAuthentication;

// ReSharper disable CheckNamespace

namespace Microsoft.Extensions.DependencyInjection;


public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     Add audience validator for app client to allow validating JWT token audience
    /// </summary>
    /// <param name="serviceCollection"></param>
    /// <returns></returns>
    public static IServiceCollection AddAppClientAudienceValidator(this IServiceCollection serviceCollection)
    {
        serviceCollection.RegisterAuthAudienceValidator<AuthClientsAuthAudienceValidator>();
        serviceCollection.RegisterAuthAudienceResolver<AuthClientsAuthAudienceResolver>();

        return serviceCollection;
    }

    /// <summary>
    ///     Add audience validator for app client to allow validating JWT token audience
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static AppClientManagerBuilder AddAppClientAudienceValidator(this AppClientManagerBuilder builder)
    {
        builder.Services.RegisterAuthAudienceValidator<AuthClientsAuthAudienceValidator>();
        builder.Services.RegisterAuthAudienceResolver<AuthClientsAuthAudienceResolver>();

        return builder;
    }
}