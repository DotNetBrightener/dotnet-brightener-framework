using System;
using System.Collections.Concurrent;

namespace DotNetBrightener.Core.DataAccess
{
    public interface IDataWorkContext
    {
        void SetContextData<T>(T contextData, string accessKey = null);

        T GetContextData<T>(string accessKey = null);
    }

    public class DataWorkContext : IDisposable, IDataWorkContext
    {
        private readonly ConcurrentDictionary<string, object> _appHostContextData = new ConcurrentDictionary<string, object>();

        public void SetContextData<T>(T contextData, string accessKey = null)
        {
            if (accessKey == null)
                accessKey = typeof(T).FullName;

            _appHostContextData.TryAdd(accessKey, contextData);
        }

        public T GetContextData<T>(string accessKey = null)
        {
            if (accessKey == null)
                accessKey = typeof(T).FullName;

            if (_appHostContextData.TryGetValue(accessKey, out var result) && result is T tResult)
                return tResult;

            return default;
        }

        public void Dispose()
        {
            _appHostContextData.Clear();
            GC.SuppressFinalize(this);
        }
    }
}