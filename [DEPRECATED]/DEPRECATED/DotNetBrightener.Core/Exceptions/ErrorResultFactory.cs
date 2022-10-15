using System;
using DotNetBrightener.Core.ApplicationShell;

namespace DotNetBrightener.Core.Exceptions;

public class ErrorResultFactory : IErrorResultFactory
{
    private readonly IAppHostContext _appHostContext;

    public ErrorResultFactory(IAppHostContext appHostContext)
    {
        _appHostContext = appHostContext;
    }

    public T InstantiateErrorResult<T>() where T : DefaultErrorResult
    {
        var errorResult = Activator.CreateInstance<T>() as DefaultErrorResult;

        errorResult.TenantName = _appHostContext.RetrieveState<string>(CoreConstants.TenantName);

        return (T) errorResult;
    }
}