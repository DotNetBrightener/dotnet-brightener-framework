using System.Net;

namespace DotNetBrightener.WebApp.CommonShared.Exceptions;

public class BaseNotFoundException<T> : ExceptionWithStatusCode where T : class
{
    public BaseNotFoundException() : this("The requested object of type " + typeof(T).Name + " could not be found")
    {

    }
    public BaseNotFoundException(string message) : base(message, HttpStatusCode.NotFound)
    {

    }
}

public class BaseNotFoundException : ExceptionWithStatusCode
{
    public BaseNotFoundException() : this("The requested resource could not be found")
    {

    }
    public BaseNotFoundException(string message) : base(message, HttpStatusCode.NotFound)
    {

    }
}