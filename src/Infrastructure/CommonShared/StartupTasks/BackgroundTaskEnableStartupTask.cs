using DotNetBrightener.Core.BackgroundTasks;
using DotNetBrightener.Core.StartupTask;
using Microsoft.Extensions.Logging;

namespace WebApp.CommonShared.StartupTasks;

public class BackgroundTaskEnableStartupTask : IStartupTask, IDependency
{
    public int Order => 100_000;

    private readonly IBackgroundTaskContainerService _backgroundTaskContainerService;
    private readonly ILogger                         _logger;

    public BackgroundTaskEnableStartupTask(IBackgroundTaskContainerService          backgroundTaskContainerService,
                                           ILogger<BackgroundTaskEnableStartupTask> logger)

    {
        _backgroundTaskContainerService = backgroundTaskContainerService;
        _logger                         = logger;
    }

    public Task Execute()
    {
        _logger.LogInformation($"Activating background task container...");

        _backgroundTaskContainerService.Activate();

        _logger.LogInformation($"Activated background task container.");

        return Task.CompletedTask;
    }
}