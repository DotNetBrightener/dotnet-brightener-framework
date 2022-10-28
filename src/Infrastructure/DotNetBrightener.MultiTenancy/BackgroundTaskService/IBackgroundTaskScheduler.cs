using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Timer = System.Timers.Timer;

namespace DotNetBrightener.MultiTenancy.BackgroundTaskService;

/// <summary>
///     Represents the service that hosts all background tasks available in system.
/// </summary>
internal interface IBackgroundTaskScheduler
{
    void Activate();

    void EnqueueTask(MethodInfo methodAction, params object [ ] parameters);
}

internal class BackgroundTaskScheduler : IBackgroundTaskScheduler, IDisposable
{
    private readonly Queue<BackgroundQueuedTask> _tasks = new Queue<BackgroundQueuedTask>();
    private readonly IServiceProvider            _serviceResolver;
    private readonly Timer                       _timer;
    private readonly ILogger                     _logger;

    public BackgroundTaskScheduler(ILogger<BackgroundTaskScheduler> logger,
                                   IServiceProvider                 backgroundServiceProvider)
    {
        _serviceResolver =  backgroundServiceProvider;
        _logger          =  logger;
        _timer           =  new Timer();
        _timer.Interval  =  TimeSpan.FromSeconds(3).TotalMilliseconds;
        _timer.Elapsed   += Elapsed;
    }

    public void Activate()
    {
        lock (_timer)
        {
            _timer.Start();
        }
    }

    public void EnqueueTask(MethodInfo methodAction, params object[] parameters)
    {
        _tasks.Enqueue(new BackgroundQueuedTask
        {
            Action     = methodAction,
            Parameters = parameters
        });
    }

    void Elapsed(object sender, ElapsedEventArgs e)
    {
        // current implementation disallows re-entrancy
        if (!Monitor.TryEnter(_timer))
        {
            return;
        }
        try
        {
            DoWork();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, $"Error while executing background task");
        }
        finally
        {
            Monitor.Exit(_timer);
        }
    }

    private void DoWork()
    {
        if (_tasks.Count == 0)
        {
            return;
        }

        if (_serviceResolver == null)
        {
            _logger.LogError($"No service provider available to resolve tasks");
            return;
        }

        var tasksList = new List<Task>();

        while (true)
        {
            var taskToRun = _tasks.Dequeue();

            if (taskToRun != null)
            {
                taskToRun.ServiceProvider = _serviceResolver;

                tasksList.Add(RunTask(taskToRun));
            }

            if (_tasks.Count == 0)
                break;
        }

        Task.WaitAll(tasksList.ToArray());
    }

    private async Task RunTask(BackgroundQueuedTask taskToRun)
    {
        var serviceResolver = taskToRun.ServiceProvider;
        if (serviceResolver == null)
            return;

        using (var backgroundScope = serviceResolver.CreateScope())
        {
            var backgroundServiceProvider = backgroundScope.ServiceProvider;

            var declaringType = taskToRun.Action.DeclaringType;

            if (declaringType == null)
                return;

            var invokingInstance = backgroundServiceProvider.TryGetService(declaringType);

            if (invokingInstance == null)
            {
                _logger.LogError($"Unable to execute background queued task. Could not find the {declaringType.FullName} instance that can invoke the scheduled method");

                return;
            }

            var isAwaitable = taskToRun.Action.ReturnType.GetMethod(nameof(Task.GetAwaiter)) != null;

            async Task ExecuteMethod()
            {
                if (isAwaitable)
                {
                    await (dynamic) taskToRun.Action.Invoke(invokingInstance, taskToRun.Parameters);

                    return;
                }


                taskToRun.Action.Invoke(invokingInstance, taskToRun.Parameters);
            }

            try
            {
                await ExecuteMethod();
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, $"Error occured when executing background queued task.");
            }
            finally
            {
                taskToRun.ServiceProvider = null;
            }
        }
    }

    public void Dispose()
    {
        _timer.Dispose();
    }
}