using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetBrightener.Caching.Memory
{
    public static class CachingEnableServiceCollectionExtensions
    {
        public static void EnableMemoryCacheService(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IMemoryCache, MemoryCache>();
            serviceCollection.AddCacheProvider<MemoryCacheProvider>();
        }
    }
}
