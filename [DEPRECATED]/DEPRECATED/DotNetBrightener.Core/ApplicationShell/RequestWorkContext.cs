using System;

namespace DotNetBrightener.Core.ApplicationShell;

public class RequestWorkContext : BaseWorkContext, IRequestWorkContext
{
    public void Dispose()
    {
        AppHostContextData.Clear();
        GC.SuppressFinalize(this);
    }
}