using Microsoft.Extensions.DependencyInjection;

namespace DotNetBrightener.Core.BackgroundTasks;

public interface IScheduler
{
    Task RunAt(DateTime tick);

    Task CancelAllCancellableTasks();

    bool IsRunning { get;  }
}

public class Scheduler : IScheduler
{
    private          IServiceScopeFactory    _scopeFactory;
    private          int                     _schedulerIterationsActiveCount;
    private readonly CancellationTokenSource _cancellationTokenSource;

    public bool IsRunning => _schedulerIterationsActiveCount > 0;

    public Scheduler(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory            = scopeFactory;
        _cancellationTokenSource = new CancellationTokenSource();
    }

    public async Task RunAt(DateTime tick)
    {
        Interlocked.Increment(ref _schedulerIterationsActiveCount);
        //bool isFirstTick = this._isFirstTick;
        //this._isFirstTick = false;
        //await RunWorkersAt(tick, isFirstTick);
        Interlocked.Decrement(ref _schedulerIterationsActiveCount);
    }

    public async Task CancelAllCancellableTasks()
    {
        if (!_cancellationTokenSource.IsCancellationRequested)
        {
            await _cancellationTokenSource.CancelAsync();
        }
    }
}