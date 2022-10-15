namespace DotNetBrightener.Core.Exceptions;

public interface IErrorResultFactory
{
    T InstantiateErrorResult<T>() where T : DefaultErrorResult;
}