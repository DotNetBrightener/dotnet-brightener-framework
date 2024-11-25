using System.Collections.Concurrent;

namespace DotNetBrightener.Caching;

public interface ICacheProvider : IBaseCacheService
{
    bool CanUse { get; }
}

internal class InternalCacheProvider : ICacheProvider
{
    private static readonly ConcurrentDictionary<string, object> Cache = new();

    public void Dispose()
    {
    }

    public T Get<T>(CacheKey key, Func<T> acquire)
    {
        if (Cache.TryGetValue(key.Key, out var cachedValue))
        {
            return (T)cachedValue;
        }

        var result = acquire();
        Set(key, result);

        return result;
    }

    public async Task<T> GetAsync<T>(CacheKey key, Func<Task<T>> acquire)
    {
        if (Cache.TryGetValue(key.Key, out var cachedValue))
        {
            if (cachedValue is T value)
                return value;
        }

        var result = await acquire();
        Set(key, result);

        return result;
    }

    public void Remove(CacheKey key)
    {
        Cache.TryRemove(key.ToString(), out _);
    }

    public void Set(CacheKey key, object data)
    {
        Cache.TryAdd(key.ToString(), data);
    }

    public bool IsSet(CacheKey key)
    {
        return Cache.ContainsKey(key.Key);
    }

    public void RemoveByPrefix(string prefix)
    {
        var keysToRemove = Cache.Keys.Where(key => key.StartsWith(prefix)).ToList();

        foreach (var key in keysToRemove)
        {
            Cache.TryRemove(key, out _);
        }
    }

    public void Clear()
    {
        Cache.Clear();
    }

    public bool CanUse => true;
}