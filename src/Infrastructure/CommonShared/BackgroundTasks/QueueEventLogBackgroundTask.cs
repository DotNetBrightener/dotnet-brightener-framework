using System.Threading.Tasks;
using DotNetBrightener.Core.BackgroundTasks;
using DotNetBrightener.Core.Logging;

namespace DotNetBrightener.WebApp.CommonShared.BackgroundTasks;

public class QueueEventLogBackgroundTask: IBackgroundTask, IDependency
{
    private readonly IQueueEventLogBackgroundProcessing _logBackgroundProcessing;

    public QueueEventLogBackgroundTask(IQueueEventLogBackgroundProcessing logBackgroundProcessing)
    {
        _logBackgroundProcessing = logBackgroundProcessing;
    }

    Task IBackgroundTask.Execute()
    {
        return _logBackgroundProcessing.Execute();
    }
}
