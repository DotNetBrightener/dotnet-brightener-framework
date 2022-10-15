using Microsoft.Extensions.DependencyInjection;

namespace DotNetBrightener.Caching.Redis;

public static class RedisCachingEnableServiceCollectionExtensions
{
    public static void EnableRedisCacheService(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<IRedisConnectionWrapper, RedisConnectionWrapper>();

        serviceCollection.AddCacheProvider<RedisCacheProvider>();
    }
}