using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace DotNetBrightener.Caching.Memory;

/// <summary>
///     Represents a provider for caching using memory
/// </summary>
public class MemoryCacheProvider : ICacheProvider
{
    public bool CanUse => true;

    #region Fields

    // Flag: Has Dispose already been called?
    private bool _disposed;

    private readonly IMemoryCache _memoryCache;

    private static readonly ConcurrentDictionary<string, CancellationTokenSource> _prefixes =
        new ConcurrentDictionary<string, CancellationTokenSource>();

    private static CancellationTokenSource _clearToken = new CancellationTokenSource();
    private readonly ILogger _logger;

    #endregion

    #region Ctor

    public MemoryCacheProvider(IMemoryCache                 memoryCache,
                               ILogger<MemoryCacheProvider> logger)
    {
        _memoryCache = memoryCache;
        _logger      = logger;
    }

    #endregion

    #region Utilities

    /// <summary>
    /// Prepare cache entry options for the passed key
    /// </summary>
    /// <param name="key">Cache key</param>
    /// <returns>Cache entry options</returns>
    private MemoryCacheEntryOptions PrepareEntryOptions(CacheKey key)
    {
        //set expiration time for the passed cache key
        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(key.CacheTime)
        };

        //add tokens to clear cache entries
        options.AddExpirationToken(new CancellationChangeToken(_clearToken.Token));

        foreach (var keyPrefix in key.Prefixes.ToList())
        {
            var tokenSource = _prefixes.GetOrAdd(keyPrefix, new CancellationTokenSource());
            options.AddExpirationToken(new CancellationChangeToken(tokenSource.Token));
        }

        return options;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Get a cached item. If it's not in the cache yet, then load and cache it
    /// </summary>
    /// <typeparam name="T">Type of cached item</typeparam>
    /// <param name="key">Cache key</param>
    /// <param name="acquire">Function to load item if it's not in the cache yet</param>
    /// <returns>The cached value associated with the specified key</returns>
    public T Get<T>(CacheKey key, Func<T> acquire)
    {
        if (key.CacheTime <= 0)
            return acquire();

        var result = _memoryCache.GetOrCreate(key.Key, entry =>
        {
            _logger.LogInformation("No result found in cache. Acquiring result...");

            entry.SetOptions(PrepareEntryOptions(key));

            return acquire();
        });

        //do not cache null value
        if (result == null)
            Remove(key);

        return result;
    }

    /// <summary>
    /// Removes the value with the specified key from the cache
    /// </summary>
    /// <param name="key">Key of cached item</param>
    public void Remove(CacheKey key)
    {
        _memoryCache.Remove(key.Key);
    }

    /// <summary>
    /// Get a cached item. If it's not in the cache yet, then load and cache it
    /// </summary>
    /// <typeparam name="T">Type of cached item</typeparam>
    /// <param name="key">Cache key</param>
    /// <param name="acquire">Function to load item if it's not in the cache yet</param>
    /// <returns>The cached value associated with the specified key</returns>
    public async Task<T> GetAsync<T>(CacheKey key, Func<Task<T>> acquire)
    {
        if (key.CacheTime <= 0)
            return await acquire();

        var result = await _memoryCache.GetOrCreateAsync(key.Key, async entry =>
        {
            _logger.LogInformation("No result found in cache. Acquiring result...");

            entry.SetOptions(PrepareEntryOptions(key));

            return await acquire();
        });

        //do not cache null value
        if (result == null)
            Remove(key);

        return result;
    }

    /// <summary>
    /// Adds the specified key and object to the cache
    /// </summary>
    /// <param name="key">Key of cached item</param>
    /// <param name="data">Value for caching</param>
    public void Set(CacheKey key, object data)
    {
        if (key.CacheTime <= 0 || data == null)
            return;

        _memoryCache.Set(key.Key, data, PrepareEntryOptions(key));
    }

    /// <summary>
    /// Gets a value indicating whether the value associated with the specified key is cached
    /// </summary>
    /// <param name="key">Key of cached item</param>
    /// <returns>True if item already is in cache; otherwise false</returns>
    public bool IsSet(CacheKey key)
    {
        return _memoryCache.TryGetValue(key.Key, out _);
    }

    /// <summary>
    /// Removes items by key prefix
    /// </summary>
    /// <param name="prefix">String key prefix</param>
    public void RemoveByPrefix(string prefix)
    {
        _prefixes.TryRemove(prefix, out var tokenSource);
        _logger.LogInformation("Removing cached item by prefix {prefix}", prefix);
        tokenSource?.Cancel();
        tokenSource?.Dispose();
    }

    /// <summary>
    /// Clear all cache data
    /// </summary>
    public void Clear()
    {
        _clearToken.Cancel();
        _clearToken.Dispose();

        _clearToken = new CancellationTokenSource();

        foreach (var prefix in _prefixes.Keys.ToList())
        {
            _prefixes.TryRemove(prefix, out var tokenSource);
            tokenSource?.Dispose();
        }
    }

    /// <summary>
    /// Dispose cache manager
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    // Protected implementation of Dispose pattern.
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            _memoryCache.Dispose();
        }

        _disposed = true;
    }

    #endregion
}

internal static class CacheKeyExtensions
{
    public static string ToMemoryCacheKey(this CacheKey key)
    {
        var prefixes = new List<string>
        {
            key.Key
        };
        
        prefixes.AddRange(key.Prefixes ?? []);

        return string.Join("::", prefixes.ToArray());
    }
}