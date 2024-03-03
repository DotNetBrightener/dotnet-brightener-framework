using DotNetBrightener.Caching;

// ReSharper disable CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class CachingEnableServiceCollectionExtensions
{
    public static IServiceCollection AddCacheProvider<TCacheManager>(this IServiceCollection serviceCollection)
        where TCacheManager : class, ICacheProvider
    {
        serviceCollection.AddSingleton<ICacheProvider, TCacheManager>();

        return serviceCollection;
    }

    public static IServiceCollection EnableCachingService(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<ICacheManager, DefaultCacheManager>();

        return serviceCollection;
    }
}