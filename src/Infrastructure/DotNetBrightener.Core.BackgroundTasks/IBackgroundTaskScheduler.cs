using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Timer = System.Timers.Timer;

namespace DotNetBrightener.Core.BackgroundTasks;

/// <summary>
///     Represents the service that hosts all background tasks available in system.
/// </summary>
public interface IBackgroundTaskScheduler
{
    void Activate();

    string EnqueueTask(MethodInfo methodAction, params object[] parameters);

    QueuedTaskResult GetTaskProcessResult(string taskIdentifier);

    void RemoveTaskResult(string taskIdentifier);
}

public class BackgroundTaskScheduler : IBackgroundTaskScheduler, IDisposable
{
    private readonly Queue<BackgroundQueuedTask>                        _tasks        = new();
    private readonly ConcurrentDictionary<string, BackgroundQueuedTask> _runningTasks = new();
    private readonly IServiceScopeFactory                               _serviceResolver;
    private readonly Timer                                              _timer;
    private readonly ILogger                                            _logger;

    public BackgroundTaskScheduler(ILogger<BackgroundTaskScheduler> logger,
                                   IServiceScopeFactory backgroundServiceProvider)
    {
        _serviceResolver = backgroundServiceProvider;
        _logger = logger;
        _timer = new Timer();
        _timer.Interval = TimeSpan.FromSeconds(3).TotalMilliseconds;
        _timer.Elapsed += Elapsed;
    }

    public void Activate()
    {
        lock (_timer)
        {
            _timer.Start();
        }
    }

    public string EnqueueTask(MethodInfo methodAction, params object[] parameters)
    {
        var queuedTask = new BackgroundQueuedTask
        {
            Action = methodAction,
            Parameters = parameters
        };
        _tasks.Enqueue(queuedTask);

        if (!_timer.Enabled)
        {
            Activate();
        }

        return queuedTask.TaskIdentifier;
    }

    public QueuedTaskResult GetTaskProcessResult(string taskIdentifier)
    {
        var queuedTask = _tasks.FirstOrDefault(_ => _.TaskIdentifier == taskIdentifier);

        if (queuedTask == null)
        {
            _runningTasks.TryGetValue(taskIdentifier, out queuedTask);
        }

        if (queuedTask == null)
            return null;

        return new QueuedTaskResult
        {
            TaskIdentifier = taskIdentifier,
            Started = queuedTask.StartedOn,
            TaskResult = queuedTask.TaskResult
        };
    }

    public void RemoveTaskResult(string taskIdentifier)
    {
        if (_runningTasks.TryGetValue(taskIdentifier, out _))
        {
            _runningTasks.TryRemove(taskIdentifier, out _);
        }
    }

    void Elapsed(object sender, ElapsedEventArgs e)
    {
        // current implementation disallows re-entrancy
        if (!Monitor.TryEnter(_timer))
        {
            _logger.LogInformation("Timer is being locked for another execution. Exiting...");
            return;
        }

        try
        {
            _logger.LogInformation("Stop and lock timer for execution.");
            _timer.Stop();
            DoWork();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, $"Error while executing background task");
        }
        finally
        {
            if (_tasks.Count > 0 &&
                !_timer.Enabled)
            {
                _timer.Start();
            }

            Monitor.Exit(_timer);
        }
    }

    private void DoWork()
    {
        if (_tasks.Count == 0)
        {
            if (_timer.Enabled)
            {
                lock (_timer)
                    _timer.Stop();
            }
            return;
        }

        var tasksList = new List<Task>();

        while (true)
        {
            var taskToRun = _tasks.Dequeue();

            if (taskToRun != null)
            {
                tasksList.Add(RunTask(taskToRun));
            }

            if (_tasks.Count == 0)
                break;
        }

        Task.WaitAll(tasksList.ToArray());
    }

    private async Task RunTask(BackgroundQueuedTask taskToRun)
    {
        taskToRun.StartedOn = DateTimeOffset.UtcNow;
        _runningTasks.TryAdd(taskToRun.TaskIdentifier, taskToRun);

        using (var backgroundScope = _serviceResolver.CreateScope())
        {
            var backgroundServiceProvider = backgroundScope.ServiceProvider;

            var declaringType = taskToRun.Action.DeclaringType;

            if (declaringType == null)
                return;

            _logger.LogInformation($"Trying to get instance of {declaringType}");
            object invokingInstance = null;

            try
            {
                invokingInstance = backgroundServiceProvider.TryGet(declaringType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error while resolving instance of {declaringType}");
            }

            if (invokingInstance is null)
            {
                _logger.LogError($"Unable to execute background queued task. Could not find the {declaringType.FullName} instance that can invoke the scheduled method");

                return;
            }

            var isAwaitable = taskToRun.Action.ReturnType.GetMethod(nameof(Task.GetAwaiter)) != null;

            Task ExecuteMethod()
            {
                _logger.LogInformation($"Executing task: {taskToRun.Action.DeclaringType?.FullName}.{taskToRun.Action.Name}()");

                if (isAwaitable)
                {
                    var invokeResult = Task.Run(async () =>
                    {
                        try
                        {
                            var result = await (dynamic)taskToRun.Action.Invoke(invokingInstance, taskToRun.Parameters);

                            return result;
                        }
                        catch
                        {
                            return null;
                        }
                    });

                    taskToRun.TaskResult = invokeResult;

                    return taskToRun.TaskResult;
                }

                taskToRun.Action.Invoke(invokingInstance, taskToRun.Parameters);

                return Task.CompletedTask;
            }

            try
            {
                await ExecuteMethod();
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, $"Error occurred when executing background queued task {taskToRun.Action.DeclaringType?.FullName}.{taskToRun.Action.Name}().");
            }
        }
    }

    public void Dispose()
    {
        _timer.Dispose();
    }
}