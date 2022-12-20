using DotNetBrightener.CommonShared.Exceptions;

namespace DotNetBrightener.CommonShared.Mvc;

public interface IUnhandledExceptionHandler
{
    public void HandleException(UnhandledExceptionContext context);
}

internal class DefaultUnhandledExceptionHandler : IUnhandledExceptionHandler
{
    public void HandleException(UnhandledExceptionContext context)
    {
            
    }
}