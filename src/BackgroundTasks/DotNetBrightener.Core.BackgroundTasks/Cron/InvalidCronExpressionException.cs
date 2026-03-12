namespace DotNetBrightener.Core.BackgroundTasks.Cron;

public class InvalidCronExpressionException(string message) : Exception(message);