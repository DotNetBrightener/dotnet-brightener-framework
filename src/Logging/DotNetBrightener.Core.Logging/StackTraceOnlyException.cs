namespace DotNetBrightener.Core.Logging;

public class StackTraceOnlyException : Exception
{
    public StackTraceOnlyException(string message)
        : base(message)
    {
    }

    public StackTraceOnlyException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
