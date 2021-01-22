using DotNetBrightener.Core.DataAccess.Abstractions;
using System;
using System.Collections.Concurrent;

namespace DotNetBrightener.Core.DataAccess
{
    public class DataWorkContext : IDisposable, IDataWorkContext
    {
        private readonly ConcurrentDictionary<string, object> _dataWorkContext = new ConcurrentDictionary<string, object>();

        public void SetContextData<T>(T contextData, string accessKey = null)
        {
            if (accessKey == null)
                accessKey = typeof(T).FullName;

            _dataWorkContext.TryAdd(accessKey, contextData);
        }

        public T GetContextData<T>(string accessKey = null)
        {
            if (accessKey == null)
                accessKey = typeof(T).FullName;

            if (_dataWorkContext.TryGetValue(accessKey, out var result) && result is T tResult)
                return tResult;

            return default;
        }

        public void Dispose()
        {
            _dataWorkContext.Clear();
            GC.SuppressFinalize(this);
        }
    }
}