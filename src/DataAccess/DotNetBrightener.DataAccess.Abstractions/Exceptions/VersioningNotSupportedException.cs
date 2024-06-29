namespace DotNetBrightener.DataAccess.Exceptions;

/// <summary>
///     Describes the exception that will be thrown when the requested entity does not support versioning
/// </summary>
/// <typeparam name="T">
///     
/// </typeparam>
public class VersioningNotSupportedException<T>(string message, Exception innerException)
    : NotSupportedException(message, innerException)
    where T : class
{
    public VersioningNotSupportedException()
        : this("The requested object of type " + typeof(T).Name + " does not support versioning")
    {

    }

    public VersioningNotSupportedException(string message)
        : this(message, null)
    {

    }
}