using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace DotNetBrightener.Core.StartupTask.StartupServices;

internal class StartupTaskExecutionHostedService : IHostedService, IDisposable
{
    private readonly IServiceScopeFactory                       _serviceScopeFactory;
    private readonly IServiceCollection                         _services;
    private readonly ILogger<StartupTaskExecutionHostedService> _logger;
    private readonly IHostApplicationLifetime                   _lifetime;

    private int _attempts;

    public StartupTaskExecutionHostedService(IServiceScopeFactory                       serviceScopeFactory,
                                             IServiceCollection                         services,
                                             ILogger<StartupTaskExecutionHostedService> logger,
                                             IHostApplicationLifetime                   lifetime)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _services            = services;
        _logger              = logger;
        _lifetime            = lifetime;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _lifetime.ApplicationStarted.Register(async () =>
        {
            await Execute();
        });
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
    }

    public void Dispose()
    {
    }

    private async Task Execute()
    {
        while (_attempts < 3)
        {
            try
            {
                using var serviceScope = _serviceScopeFactory.CreateScope();

                var serviceProvider = serviceScope.ServiceProvider;

                await ExecuteStartupTasks(serviceProvider, _logger);

                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                                 "Failed to resolve start up tasks after {attempts} attempts. Retrying...",
                                 _attempts);
                _attempts++;
            }
        }
    }


    /// <summary>
    ///     Executes the startup tasks from the given service provider
    /// </summary>
    /// <param name="serviceProvider">
    ///     The <see cref="IServiceProvider"/> to extract the startup tasks and execute them
    /// </param>
    /// <param name="logger"></param>
    /// <returns></returns>
    private async Task ExecuteStartupTasks(IServiceProvider serviceProvider,
                                           ILogger          logger)
    {
        Type[] startupTaskTypes = Array.Empty<Type>();

        try
        {
            using var serviceScope = serviceProvider.CreateScope();

            startupTaskTypes = serviceScope.ServiceProvider
                                           .GetServices<IStartupTask>()
                                           .OrderBy(_ => _.Order)
                                           .Select(_ => _.GetType())
                                           .ToArray();
        }
        catch (InvalidOperationException ex)
        {
            Interlocked.Increment(ref _attempts);

            throw;
        }

        if (startupTaskTypes.Length == 0)
        {
            logger.LogDebug("No start up tasks found.");

            return;
        }

        logger.LogInformation("Found {tasks} start up tasks: {@tasksList}...",
                              startupTaskTypes.Length,
                              startupTaskTypes.Select(taskType => taskType.Name));

        var synchronousTasks = startupTaskTypes
                              .Where(taskType => taskType.IsAssignableTo(typeof(ISynchronousStartupTask)))
                              .ToArray();

        var asynchronousTasks = startupTaskTypes.Except(synchronousTasks)
                                                .ToArray();

        Stopwatch sw = Stopwatch.StartNew();

        if (synchronousTasks.Length > 0)
        {
            logger.LogInformation("Executing {numberOfTasks} synchronous tasks", synchronousTasks.Length);

            foreach (var startupTaskType in synchronousTasks)
            {
                await ExecuteTask(serviceProvider, startupTaskType, logger);
            }
        }

        if (asynchronousTasks.Length > 0)
        {
            logger.LogInformation("Executing {numberOfTasks} asynchronous tasks. They will be executed in parallel.",
                                  asynchronousTasks.Length);

            await Parallel.ForEachAsync(asynchronousTasks,
                                        async (type, ct) =>
                                        {
                                            await ExecuteTask(serviceProvider, type, logger);
                                        });
        }

        sw.Stop();

        logger?.LogInformation("Start up tasks execution finished after {Elapsed}. " +
                               "Total {numberOfTasks} have executed.",
                               sw.Elapsed,
                               startupTaskTypes.Length);
    }

    private async Task ExecuteTask(IServiceProvider serviceProvider,
                                   Type             startupTaskType,
                                   ILogger          logger)
    {
        var       taskType            = startupTaskType.Name;
        using var backgroundTaskScope = serviceProvider.CreateScope();

        if (backgroundTaskScope.ServiceProvider
                               .TryGet(startupTaskType) is not IStartupTask taskInstance)
        {
            logger.LogWarning("Cannot resolve task {taskType} from the service collection. Skipping...",
                              taskType);

            return;
        }


        logger.LogInformation("Starting task {taskType} execution {syncState}...",
                              taskType,
                              taskInstance is ISynchronousStartupTask
                                  ? "synchronously"
                                  : "asynchronously");

        var sw = Stopwatch.StartNew();

        await taskInstance.Execute();

        sw.Stop();

        logger.LogInformation("Finished task {taskType} execution after {Elapsed}.", taskType, sw.Elapsed);
    }
}