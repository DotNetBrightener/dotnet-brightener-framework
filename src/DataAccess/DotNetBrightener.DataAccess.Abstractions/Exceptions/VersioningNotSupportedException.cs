using System;

namespace DotNetBrightener.DataAccess.Exceptions;

/// <summary>
///     Describes the exception that will be thrown when the requested entity does not support versioning
/// </summary>
/// <typeparam name="T">
///     
/// </typeparam>
public class VersioningNotSupportedException<T> : NotSupportedException where T : class
{
    public VersioningNotSupportedException()
        : this("The requested object of type " + typeof(T).Name + " does not support versioning")
    {

    }

    public VersioningNotSupportedException(string message)
        : this(message, null)
    {

    }

    public VersioningNotSupportedException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}