using System;
using System.Collections.Concurrent;

namespace DotNetBrightener.Core.ApplicationShell
{
    public interface IWorkContext
    {
        /// <summary>
        ///     Stores the given object to the application host context
        /// </summary>
        /// <param name="stateKey">The key to store and retrieve the object</param>
        /// <param name="value">The object to store to the application host</param>
        void StoreState(string stateKey, object value);

        /// <summary>
        ///     Stores the given object to the application host context using the <typeparamref name="T"/> as key name
        /// </summary>
        /// <typeparam name="T">The type of object to store</typeparam>
        /// <param name="value">The object to store to the application host</param>
        void StoreState<T>(T value);

        /// <summary>
        ///     Retrieves the object from application host.
        /// </summary>
        /// <typeparam name="T">The type of object expected to return</typeparam>
        /// <param name="stateKey">The key of the stored object. If not specified, the type of object will be used</param>
        /// <returns>The stored object if found, otherwise, <c>null</c></returns>
        T RetrieveState<T>(string stateKey = null);
    }

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
}