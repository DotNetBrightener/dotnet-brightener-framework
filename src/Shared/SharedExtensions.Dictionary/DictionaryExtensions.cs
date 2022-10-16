using System.Collections.Specialized;
using System.Linq;
using System.Reflection;

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


internal static class DictionaryToQueryModelExtensions
{
    /// <summary>
    ///     Convert the dictionary of string: string to the query model of <typeparamref name="TQueryModel" />
    /// </summary>
    /// <typeparam name="TQueryModel">Type of the model to convert to</typeparam>
    /// <returns>
    ///     The instance of <typeparamref name="TQueryModel" /> from the provided dictionary
    /// </returns>
    public static TQueryModel ToQueryModel<TQueryModel>(this Dictionary<string, string> queryDictionary)
    {
        var queryModel = Activator.CreateInstance<TQueryModel>();

        if (queryDictionary == null ||
            !queryDictionary.Any())
        {
            return queryModel;
        }

        var targetType = typeof(TQueryModel);

        foreach (var keyValuePair in queryDictionary)
        {
            if (targetType.RetrieveMemberInfo(keyValuePair.Key) is PropertyInfo property)
            {
                var propertyValue = Convert.ChangeType(keyValuePair.Value, property.PropertyType);
                property.SetValue(queryModel, propertyValue);
            }
        }

        return queryModel;
    }

    /// <summary>
    ///     Convert the dictionary of string: string to the query model of <typeparamref name="TQueryModel" />
    /// </summary>
    /// <typeparam name="TQueryModel">Type of the model to convert to</typeparam>
    /// <returns>
    ///     The instance of <typeparamref name="TQueryModel" /> from the provided dictionary
    /// </returns>
    public static TQueryModel ToQueryModel<TQueryModel>(this NameValueCollection queryDictionary)
    {
        var dictionary = queryDictionary.Cast<string>()
                                        .ToDictionary(_ => _,
                                                      _ => queryDictionary[_]);

        return dictionary.ToQueryModel<TQueryModel>();
    }
}