using DotNetBrightener.WebApp.CommonShared.Exceptions;

namespace DotNetBrightener.WebApp.CommonShared.Mvc;

public interface IUnhandledExceptionHandler: IDependency
{
    public void HandleException(UnhandledExceptionContext context);
}

internal class DefaultUnhandledExceptionHandler : IUnhandledExceptionHandler
{
    public void HandleException(UnhandledExceptionContext context)
    {
            
    }
}