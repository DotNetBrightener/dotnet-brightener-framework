using DotNetBrightener.Infrastructure.ApiKeyAuthentication.Constants;
using DotNetBrightener.Infrastructure.ApiKeyAuthentication.Middlewares;
using DotNetBrightener.Infrastructure.ApiKeyAuthentication.Permissions;
using DotNetBrightener.Infrastructure.ApiKeyAuthentication.Services;
using Microsoft.Extensions.DependencyInjection.Extensions;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public class ApiKeyAuthConfigurationBuilder
{
    internal IServiceCollection ServiceCollection { get; set; }

    public ApiKeyAuthConfigurationBuilder UseApiTokenStore<TApiStoreService>()
        where TApiStoreService : class, IApiKeyStoreService
    {
        ServiceCollection.RemoveAll<IApiKeyStoreService>();
        ServiceCollection.AddScoped<IApiKeyStoreService, TApiStoreService>();

        return this;
    }
}

public static class ServiceCollectionExtensions
{
    public static ApiKeyAuthConfigurationBuilder AddApiKeyAuthentication(this IMvcBuilder mvcBuilder)
    {
        var apiKeyAuthConfBuider = new ApiKeyAuthConfigurationBuilder
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

        return apiKeyAuthConfBuider;
    }
}