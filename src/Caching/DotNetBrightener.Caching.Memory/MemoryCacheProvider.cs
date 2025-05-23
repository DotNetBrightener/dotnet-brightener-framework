﻿using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace DotNetBrightener.Caching.Memory;

/// <summary>
///     Represents a provider for caching using memory
/// </summary>
public class MemoryCacheProvider(
    IMemoryCache                 memoryCache,
    ILogger<MemoryCacheProvider> logger)
    : ICacheProvider
{
    public bool CanUse => true;

    // Flag: Has Dispose() already been called?
    private bool _disposed;

    private static readonly ConcurrentDictionary<string, CancellationTokenSource> Prefixes = new();

    private static   CancellationTokenSource _clearToken = new();
    private readonly ILogger                 _logger     = logger;

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
            var tokenSource = Prefixes.GetOrAdd(keyPrefix, new CancellationTokenSource());
            options.AddExpirationToken(new CancellationChangeToken(tokenSource.Token));
        }

        return options;
    }

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

        var result = memoryCache.GetOrCreate(key.Key,
                                             entry =>
                                             {
                                                 _logger
                                                    .LogInformation("No result found in cache. Acquiring result...");

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
        memoryCache.Remove(key.Key);
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

        var result = await memoryCache.GetOrCreateAsync(key.Key,
                                                        async entry =>
                                                        {
                                                            _logger
                                                               .LogInformation("No result found in cache. Acquiring result...");

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
        if (key.CacheTime <= 0 ||
            data == null)
            return;

        memoryCache.Set(key.Key, data, PrepareEntryOptions(key));
    }

    /// <summary>
    /// Gets a value indicating whether the value associated with the specified key is cached
    /// </summary>
    /// <param name="key">Key of cached item</param>
    /// <returns>True if item already is in cache; otherwise false</returns>
    public bool IsSet(CacheKey key)
    {
        return memoryCache.TryGetValue(key.Key, out _);
    }

    /// <summary>
    /// Removes items by key prefix
    /// </summary>
    /// <param name="prefix">String key prefix</param>
    public void RemoveByPrefix(string prefix)
    {
        Prefixes.TryRemove(prefix, out var tokenSource);
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

        foreach (var prefix in Prefixes.Keys.ToList())
        {
            Prefixes.TryRemove(prefix, out var tokenSource);
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
            memoryCache.Dispose();
        }

        _disposed = true;
    }
}