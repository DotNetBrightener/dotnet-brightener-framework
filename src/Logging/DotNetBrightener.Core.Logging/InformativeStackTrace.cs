namespace DotNetBrightener.Core.Logging;

public class InformativeStackTrace : Exception
{
    public InformativeStackTrace(string message)
        : base(message)
    {
    }

    public InformativeStackTrace(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
