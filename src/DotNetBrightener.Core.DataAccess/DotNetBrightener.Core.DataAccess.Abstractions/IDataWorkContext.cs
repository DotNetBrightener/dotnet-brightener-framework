namespace DotNetBrightener.Core.DataAccess.Abstractions
{
    /// <summary>
    ///     Provides the context for the data access.
    /// </summary>
    public interface IDataWorkContext
    {
        void SetContextData<T>(T contextData, string accessKey = null);

        T GetContextData<T>(string accessKey = null);
    }
}