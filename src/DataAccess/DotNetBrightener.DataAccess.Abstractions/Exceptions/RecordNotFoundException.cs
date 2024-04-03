namespace DotNetBrightener.DataAccess.Exceptions;

/// <summary>
///     Describes the exception that will be thrown when a requested entity record cannot be found
/// </summary>
/// <typeparam name="T"></typeparam>
public class RecordNotFoundException<T> : Exception where T : class
{
    public RecordNotFoundException() : this("The requested object of type " + typeof(T).Name + " could not be found")
    {

    }

    public RecordNotFoundException(string message) : this(message, null)
    {

    }

    public RecordNotFoundException(string message, Exception innerException) : base(message, innerException)
    {
    }
}