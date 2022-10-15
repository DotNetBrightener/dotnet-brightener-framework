namespace System;

internal static class ExceptionExtensions
{
    public static string GetFullExceptionMessage(this Exception exception)
    {
        var fullMessage = exception.Message;

        while ((exception = exception.InnerException) != null)
        {
            fullMessage += "\r\n";
            fullMessage += exception.Message;
        }

        return fullMessage;
    }
}