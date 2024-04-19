namespace DotNetBrightener.Core.BackgroundTasks.Cron;

public class InvalidCronExpressionException : Exception
{
    public InvalidCronExpressionException(string message) : base(message)
    {
    }
}