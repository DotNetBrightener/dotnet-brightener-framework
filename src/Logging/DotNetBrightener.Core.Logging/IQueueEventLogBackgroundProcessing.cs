namespace DotNetBrightener.Core.Logging;

public interface IQueueEventLogBackgroundProcessing
{
    Task Execute();
}

public class NullEventLogBackgroundProcessing : IQueueEventLogBackgroundProcessing
{
    public Task Execute()
    {
        throw new NotImplementedException();
    }
}