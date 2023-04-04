using DotNetBrightener.Infrastructure.ApiKeyAuthentication.Middlewares;
using DotNetBrightener.Infrastructure.ApiKeyAuthentication.Permissions;
using DotNetBrightener.Infrastructure.ApiKeyAuthentication.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection.Extensions;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public class ApiKeyAuthConfigurationBuilder
{
    internal IServiceCollection ServiceCollection { get; set; }

    internal IMvcBuilder MvcBuilder { get; set; }

    public ApiKeyAuthConfigurationBuilder UseApiStore<TApiStoreService>()
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
            MvcBuilder        = mvcBuilder,
            ServiceCollection = mvcBuilder.Services
        };

        mvcBuilder.RegisterMeAsMvcModule();
        mvcBuilder.Services.RegisterPermissionProvider<ApiKeyAuthPermissions>();

        return apiKeyAuthConfBuider;
    }

    public static void UseApiKeyAuthentication(this IApplicationBuilder appBuilder)
    {
        appBuilder.UseMiddleware<ApiKeyAuthMiddleware>();
    }
}