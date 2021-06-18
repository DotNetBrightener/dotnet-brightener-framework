using DotNetBrightener.Caching;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class CachingEnableServiceCollectionExtensions
    {
        public static void AddCacheProvider<TCacheManager>(this IServiceCollection serviceCollection)
            where TCacheManager : class, ICacheProvider
        {
            serviceCollection.AddSingleton<ICacheProvider, TCacheManager>();
        }

        public static void EnableCachingService(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<ICacheManager, DefaultCacheManager>();
        }
    }
}