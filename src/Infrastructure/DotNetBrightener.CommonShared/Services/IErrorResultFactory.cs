using System;
using DotNetBrightener.CommonShared.Models;

namespace DotNetBrightener.CommonShared.Services;

public interface IErrorResultFactory
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