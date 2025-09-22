using DotNetBrightener.Core.BackgroundTasks;
using Microsoft.Extensions.DependencyInjection;

namespace WebApp.CommonShared.AsyncTasks;

public interface IAsyncTaskScheduler : IDependency
{
    Task<Guid> ScheduleTask<TTaskExecutor, TInput>(AsyncTaskContext context)
        where TInput : class
        where TTaskExecutor : class, IAsyncTask<TInput>;
}

public class AsyncTaskScheduler(
    IAsyncTaskContainer  taskContainer,
    IScheduler           scheduler,
    IServiceScopeFactory serviceScopeFactory) : IAsyncTaskScheduler
{
    public async Task<Guid> ScheduleTask<TTaskExecutor, TInput>(AsyncTaskContext context)
        where TInput : class
        where TTaskExecutor : class, IAsyncTask<TInput>
    {
        context.ScheduledAt = DateTimeOffset.UtcNow;

        var taskId = await taskContainer.ScheduleTask(context);

        var executeMethod = typeof(AsyncTaskScheduler).GetMethodWithName("ExecuteTask");

        var executeMethodWithGeneric = executeMethod.MakeGenericMethod(typeof(TInput));

        scheduler.ScheduleTask(executeMethodWithGeneric,
                               typeof(TTaskExecutor),
                               context)
                 .PreventOverlapping(taskId.ToString())
                 .Once();

        return taskId;
    }

    private async Task ExecuteTask<TInput>(Type             requiredTaskType,
                                           AsyncTaskContext context)
        where TInput : class
    {
        await using (var scope = serviceScopeFactory.CreateAsyncScope())
        {
            var taskExecutor = scope.ServiceProvider.TryGet(requiredTaskType);
            
            if (taskExecutor is not IAsyncTask<TInput> executor ||
                context.Input is not TInput input)
            {
                return;
            }

            context.StartedAt = DateTimeOffset.UtcNow;
            dynamic taskResult = null;

            try
            {
                taskResult = await executor.Execute(input, context);
            }
            catch (Exception ex)
            {
                context.Errors = ex.GetFullExceptionMessage();
            }
            finally
            {
                context.Result      = taskResult;
                context.CompletedAt = DateTimeOffset.UtcNow;
            }
        }
    }
}