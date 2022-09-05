using System;
using System.Collections.Generic;
// ReSharper disable CheckNamespace

namespace Microsoft.AspNetCore.Http;

internal static class HttpContextExtensions
{
    /// <summary>
    ///		Stores a value to the current HttpContext to retrieve later
    /// </summary>
    /// <remarks>
    ///		The stored value will be retrieved with the key is typeof(<typeparamref name="TItem"/>)
    /// </remarks>
    /// <typeparam name="TItem">
    ///		The type of object to store to the HttpContext, which acts as the key to retrieve
    /// </typeparam>
    /// <param name="httpContext"></param>
    /// <param name="value">The value to store</param>
    ///  <exception cref="InvalidOperationException">
    /// 		Thrown if the HttpContext is not available
    ///  </exception>
    public static void StoreValue<TItem>(this HttpContext httpContext, TItem value)
    {
        if (httpContext?.Items == null)
            throw new InvalidOperationException($"No HttpContext available");

        if (!httpContext.Items.TryAdd(typeof(TItem), value)) // cannot add due to the key existence
        {
            httpContext.Items[typeof(TItem)] = value; // try to override the value
        }
    }

    /// <summary>
    ///		Retrieves the stored object from the current HttpContext, using the key of typeof(<typeparamref name="TItem"/>)
    /// </summary>
    /// <typeparam name="TItem">
    ///		The type of stored object, which acts as the key to retrieve the object
    /// </typeparam>
    /// <param name="httpContext"></param>
    /// <returns>
    ///		The <typeparamref name="TItem"/> object if found, otherwise, <c>null</c>
    /// </returns>
    /// <exception cref="InvalidOperationException">
    ///		Thrown if the HttpContext is not available
    /// </exception>
    public static TItem RetrieveValue<TItem>(this HttpContext httpContext)
    {
        if (httpContext?.Items == null)
        {
            return default;
        }

        if (httpContext.Items.TryGetValue(typeof(TItem), out var itemObject) &&
            itemObject is TItem result)
            return result;

        return default;
    }


    ///  <summary>
    /// 		Stores a value to the current HttpContext using the given <see cref="storeKey"/>. Use that to retrieve later
    ///  </summary>
    ///  <typeparam name="TItem">
    /// 		The type of object to store to the HttpContext, which acts as the key to retrieve
    ///  </typeparam>
    ///  <param name="httpContext"></param>
    ///  <param name="storeKey">The key for storing the object</param>
    ///  <param name="value">The value to store</param>
    ///  <exception cref="InvalidOperationException">
    /// 		Thrown if the HttpContext is not available
    ///  </exception>
    public static void StoreValue<TItem>(this HttpContext httpContext, string storeKey, TItem value)
    {
        if (httpContext?.Items == null)
            throw new InvalidOperationException($"No HttpContext available");

        if (!httpContext.Items.TryAdd(storeKey, value)) // cannot add due to the key existence
        {
            httpContext.Items[storeKey] = value; // try to override the value
        }
    }

    ///  <summary>
    /// 		Retrieves the stored object from the current HttpContext using the given <see cref="storeKey"/>
    ///  </summary>
    ///  <typeparam name="TItem">
    /// 		The type of stored object expected to retrieve
    ///  </typeparam>
    ///  <param name="httpContext"></param>
    ///  <param name="storeKey">The key for retrieving the object</param>
    ///  <returns>
    /// 		The <typeparamref name="TItem"/> object if found, otherwise, <c>null</c>
    ///  </returns>
    ///  <exception cref="InvalidOperationException">
    /// 		Thrown if the HttpContext is not available
    ///  </exception>
    public static TItem RetrieveValue<TItem>(this HttpContext httpContext, string storeKey)
    {
        if (httpContext?.Items == null)
        {
            return default;
        }

        if (httpContext.Items.TryGetValue(storeKey, out var itemObject) &&
            itemObject is TItem result)
            return result;

        return default;
    }
}