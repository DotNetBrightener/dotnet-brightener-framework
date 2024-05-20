using DotNetBrightener.Infrastructure.ApiKeyAuthentication;
using DotNetBrightener.Infrastructure.ApiKeyAuthentication.Constants;
using DotNetBrightener.Infrastructure.ApiKeyAuthentication.Middlewares;
using DotNetBrightener.Infrastructure.ApiKeyAuthentication.Permissions;
using DotNetBrightener.Infrastructure.ApiKeyAuthentication.Services;
using Microsoft.Extensions.DependencyInjection.Extensions;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static ApiKeyAuthConfigurationBuilder AddApiKeyAuthentication(this IMvcBuilder mvcBuilder)
    {
        var apiKeyAuthConfBuilder = new ApiKeyAuthConfigurationBuilder
        {
            ServiceCollection = mvcBuilder.Services
        };

        mvcBuilder.RegisterMeAsMvcModule();
        mvcBuilder.Services
                  .RegisterPermissionProvider<ApiKeyAuthPermissions>();

        mvcBuilder.Services
                  .AddScoped<ApiKeyAuthenticationHandler>();

        mvcBuilder.Services
                  .AddAuthentication()
                  .AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(ApiKeyAuthenticationOptions.AuthenticationScheme, null);

        return apiKeyAuthConfBuilder;
    }

    public static ApiKeyAuthConfigurationBuilder UseApiTokenStore<TApiStoreService>(this ApiKeyAuthConfigurationBuilder builder)
        where TApiStoreService : class, IApiKeyStoreService
    {
        builder.ServiceCollection.RemoveAll<IApiKeyStoreService>();
        builder.ServiceCollection.AddScoped<IApiKeyStoreService, TApiStoreService>();

        return builder;
    }
}