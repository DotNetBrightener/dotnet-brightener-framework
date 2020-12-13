using DotNetBrightener.Core.Exceptions;

namespace DotNetBrightener.Core.Mvc
{
    public interface IUnhandleExceptionHandler
    {
        void HandleException(UnhandledExceptionContext context);
    }

    internal class DefaultUnhandledExceptionHandler : IUnhandleExceptionHandler
    {
        public void HandleException(UnhandledExceptionContext context)
        {
            
        }
    }
}