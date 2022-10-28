using DotNetBrightener.Core.BackgroundTasks;
using DotNetBrightener.Core.StartupTask;
using System.Threading.Tasks;

namespace DotNetBrightener.CommonShared.StartupTasks;

public class BackgroundTaskEnableStartupTask : IStartupTask
{
    public int Order => 1000;

    private readonly IBackgroundTaskContainerService _backgroundTaskContainerService;
    private readonly IBackgroundTaskScheduler        _backgroundTaskScheduler;

    public BackgroundTaskEnableStartupTask(IBackgroundTaskContainerService backgroundTaskContainerService,
                                           IBackgroundTaskScheduler        backgroundTaskScheduler)
    {
        _backgroundTaskContainerService = backgroundTaskContainerService;
        _backgroundTaskScheduler        = backgroundTaskScheduler;
    }

    public Task Execute()
    {
        _backgroundTaskScheduler.Activate();
        _backgroundTaskContainerService.Activate();

        return Task.CompletedTask;
    }
}