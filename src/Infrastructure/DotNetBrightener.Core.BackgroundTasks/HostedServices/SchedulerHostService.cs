using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DotNetBrightener.Core.BackgroundTasks.HostedServices;

internal class SchedulerHostedService : IHostedService, IDisposable
{
    private readonly IScheduler                      _scheduler;
    private readonly ILogger<SchedulerHostedService> _logger;
    private readonly IHostApplicationLifetime        _lifetime;

    private Timer _timer;
    private bool _schedulerEnabled = true;

    public SchedulerHostedService(IScheduler scheduler,
                                  ILogger<SchedulerHostedService> logger,
                                  IHostApplicationLifetime lifetime)
    {
        _scheduler = scheduler;
        _logger = logger;
        _lifetime = lifetime;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _lifetime.ApplicationStarted.Register(InitializeAfterAppStarted);

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

        var now = DateTime.UtcNow;

        await _scheduler.RunAt(now);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        // stop the scheduler so there will be no more task 
        _schedulerEnabled = false;

        _timer?.Change(Timeout.Infinite, 0);

        await _scheduler.CancelAllCancellableTasks();

        if (_scheduler.IsRunning)
        {
            _logger.LogWarning("There are still running tasks...");
        }

        while (_scheduler.IsRunning)
        {
            await Task.Delay(50, CancellationToken.None);
        }
    }

    public void Dispose()
    {
        _timer?.Dispose();
        _logger.LogInformation("Scheduler Host Service is now stopped.");
    }
}