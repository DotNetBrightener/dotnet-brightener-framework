namespace DotNetBrightener.Caching;

public class DefaultCacheManager : ICacheManager
{
    private readonly ICacheProvider _cacheProvider;

    public DefaultCacheManager(IEnumerable<ICacheProvider> cacheProviders)
    {
        _cacheProvider = cacheProviders.FirstOrDefault(_ => _.CanUse) ??
                         new InternalCacheProvider();
    }

    public T Get<T>(CacheKey key, Func<T> acquire)
    {
        return _cacheProvider.Get(key, acquire);
    }

    public Task<T> GetAsync<T>(CacheKey key, Func<Task<T>> acquire)
    {
        return _cacheProvider.GetAsync(key, acquire);
    }

    public void Remove(CacheKey key)
    {
        _cacheProvider.Remove(key);
    }

    public void Set(CacheKey key, object data)
    {
        _cacheProvider.Set(key, data);
    }

    public bool IsSet(CacheKey key)
    {
        return _cacheProvider.IsSet(key);
    }

    public void RemoveByPrefix(string prefix)
    {
        _cacheProvider.RemoveByPrefix(prefix);
    }

    public void Clear()
    {
        _cacheProvider.Clear();
    }

    public void Dispose()
    {
    }
}