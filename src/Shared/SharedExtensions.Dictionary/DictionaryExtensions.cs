namespace System.Collections.Generic;

internal static class DictionaryExtensions
{
    public static TValue GetOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
    {
        TValue obj;
        return dictionary.TryGetValue(key, out obj) ? obj : default;
    }

    public static TValue GetOrDefault<TKey, TValue>(this IDictionary<TKey, object> dictionary, TKey key)
    {
        return dictionary.TryGetValue(key, out var obj) && obj is TValue result ? result : default;
    }
}