using DotNetBrightener.Core.Exceptions;

namespace DotNetBrightener.Core.Mvc;

public interface IUnhandleExceptionHandler
{
    void HandleException(UnhandledExceptionContext context);
}