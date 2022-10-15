using System;
using System.Collections.Concurrent;

namespace DotNetBrightener.Core.ApplicationShell;

public abstract class BaseWorkContext : IWorkContext
{
    protected readonly ConcurrentDictionary<string, object> AppHostContextData = new ConcurrentDictionary<string, object>();

    public virtual void StoreState(string stateKey, object value)
    {
        AppHostContextData.TryAdd(stateKey, value);
    }

    public virtual void StoreState<T>(T value)
    {
        AppHostContextData.TryAdd(typeof(T).FullName, value);
    }

    public virtual T RetrieveState<T>(string stateKey = null)
    {
        if (string.IsNullOrEmpty(stateKey))
        {
            stateKey = typeof(T).FullName;
        }

        if (string.IsNullOrEmpty(stateKey))
            throw new ArgumentNullException($"The state key cannot be null");

        if (AppHostContextData.TryGetValue(stateKey, out var value) &&
            value is T tValue)
        {
            return tValue;
        }

        return default;
    }
}