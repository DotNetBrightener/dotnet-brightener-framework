using System;
using DotNetBrightener.WebApp.CommonShared.Models;

namespace DotNetBrightener.WebApp.CommonShared.Services;

public interface IErrorResultFactory: IDependency
{
    T InstantiateErrorResult<T>() where T : DefaultErrorResult;
}

public class ErrorResultFactory : IErrorResultFactory
{
    public T InstantiateErrorResult<T>() where T : DefaultErrorResult
    {
        var errorResult = Activator.CreateInstance<T>() as DefaultErrorResult;
            
        return (T) errorResult;
    }
}