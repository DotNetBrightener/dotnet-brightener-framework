using System;
using System.Collections.Concurrent;
using DotNetBrightener.Core.DataAccess.Abstractions;

namespace DotNetBrightener.Core.DataAccess.EF
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