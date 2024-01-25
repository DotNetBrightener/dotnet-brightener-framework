using System;
using System.Reflection.Metadata.Ecma335;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Timer = System.Timers.Timer;

namespace DotNetBrightener.Core.BackgroundTasks;

/// <summary>
/// Represents the service that hosts all background tasks available in system.
/// </summary>
public interface IBackgroundTaskContainerService
{
    TimeSpan Interval { get; set; }

    void Activate();

    void Terminate();
}

public class BackgroundTaskContainerService : IBackgroundTaskContainerService, IDisposable
{
    private readonly IServiceScopeFactory     _serviceResolver;
    private readonly IBackgroundTaskScheduler _backgroundTaskScheduler;
    private readonly Timer                    _timer;
    private readonly ILogger                  _logger;

    public BackgroundTaskContainerService(IServiceScopeFactory                    serviceScopeFactory,
                                          IBackgroundTaskScheduler                backgroundTaskScheduler,
                                          ILogger<BackgroundTaskContainerService> logger)
    {
        _serviceResolver         = serviceScopeFactory;
        _backgroundTaskScheduler = backgroundTaskScheduler;
        _logger                  = logger;
        _timer                   = new Timer();

        Interval = TimeSpan.FromSeconds(30);

        _timer.Elapsed += Elapsed;
    }

    public TimeSpan Interval
    {
        get => TimeSpan.FromMilliseconds(_timer.Interval);
        set => _timer.Interval = value.TotalMilliseconds;
    }

    public void Activate()
    {
        lock (_timer)
        {
            _timer.Start();
        }

        _backgroundTaskScheduler.Activate();
    }

    public void Terminate()
    {
        lock (_timer)
        {
            _timer.Stop();
        }
    }

    void Elapsed(object sender, ElapsedEventArgs e)
    {
        // current implementation disallows re-entrancy
        if (!Monitor.TryEnter(_timer))
            return;

        try
        {
            if (_timer.Enabled)
            {
                _timer.Stop();
                DoWork().Wait();
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, $"Error while executing background task");
        }
        finally
        {
            _timer.Start();
            Monitor.Exit(_timer);
        }
    }

    private Task DoWork()
    {
        using var backgroundScope = _serviceResolver.CreateScope();

        var       manager         = backgroundScope.ServiceProvider
                                                   .GetRequiredService<IBackgroundTaskRunner>();

        return manager.DoWork();
    }

    public void Dispose()
    {
        _timer.Dispose();
    }
}