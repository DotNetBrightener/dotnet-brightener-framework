using System;

namespace DotNetBrightener.Core.ApplicationShell
{
    /// <summary>
    ///     Represents the context of the application host
    /// </summary>
    public interface IAppHostContext : IWorkContext
    {
    }

    public class AppHostContext : BaseWorkContext, IAppHostContext
    {
    }

    /// <summary>
    ///     Represents the context of the application host at the request scope level
    /// </summary>
    public interface IRequestWorkContext : IWorkContext, IDisposable
    {
    }

    public class RequestWorkContext : BaseWorkContext, IRequestWorkContext
    {
        public void Dispose()
        {
            AppHostContextData.Clear();
            GC.SuppressFinalize(this);
        }
    }
}