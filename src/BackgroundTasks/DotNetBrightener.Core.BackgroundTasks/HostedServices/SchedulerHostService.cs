using DotNetBrightener.Core.BackgroundTasks.Internal;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DotNetBrightener.Core.BackgroundTasks.HostedServices;

internal class SchedulerHostedService(
    IScheduler                      scheduler,
    ILogger<SchedulerHostedService> logger,
    IHostApplicationLifetime        lifetime,
    IDateTimeProvider               dateTimeProvider)
    : IHostedService, IDisposable
{
    private Timer _timer;
    private bool  _schedulerEnabled = true;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        lifetime.ApplicationStarted.Register(InitializeAfterAppStarted);

        return Task.CompletedTask;
    }

    private void InitializeAfterAppStarted()
    {
        _timer = new Timer(ExecuteTask, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
    }

    private async void ExecuteTask(object state)
    {
        // if the scheduler is told to stop, we should not run any more tasks.
        if (!_schedulerEnabled)
            return;

        var now = dateTimeProvider.UtcNow;


        logger.LogDebug("Temporarily disabling scheduler at {now} to execute tasks", now);
        _schedulerEnabled = false;

        _timer?.Change(Timeout.Infinite, 0);

        logger.LogDebug("Scheduler is running at {now}", dateTimeProvider.UtcNow);
        await scheduler.RunAt(now);

        _timer?.Change(TimeSpan.Zero, TimeSpan.FromSeconds(1));

        logger.LogDebug("Re-enabling scheduler at {now} to allow tasks coming", dateTimeProvider.UtcNow);
        _schedulerEnabled = true;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Signalling Scheduler Host Service to stop...");

        // stop the scheduler so there will be no more task 
        _schedulerEnabled = false;

        _timer?.Change(Timeout.Infinite, 0);

        await scheduler.CancelAllCancellableTasks();

        if (scheduler.IsRunning)
        {
            logger.LogWarning("There are still running tasks...");
        }

        while (scheduler.IsRunning)
        {
            await Task.Delay(50, CancellationToken.None);
        }

        logger.LogInformation("Scheduler Host Service is stopping...");
    }

    public void Dispose()
    {
        _timer?.Dispose();
        logger.LogInformation("Scheduler Host Service is now stopped.");
    }
}