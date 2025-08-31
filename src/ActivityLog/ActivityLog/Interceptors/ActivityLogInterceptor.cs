using ActivityLog.ActionFilters;
using ActivityLog.Configuration;
using ActivityLog.Internal;
using ActivityLog.Models;
using ActivityLog.Services;
using Castle.DynamicProxy;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Reflection;

namespace ActivityLog.Interceptors;

/// <summary>
/// Castle DynamicProxy interceptor for capturing method execution details
/// </summary>
public class ActivityLogInterceptor(
    IActivityLogService                activityLogService,
    IActivityLogContextProvider        contextProvider,
    IActivityLogSerializer             serializer,
    IOptions<ActivityLogConfiguration> configuration,
    ILogger<ActivityLogInterceptor>    logger)
    : IInterceptor
{
    private readonly ActivityLogConfiguration _configuration = configuration.Value;

    public void Intercept(IInvocation invocation)
    {
        // Check if logging is enabled
        if (!_configuration.IsEnabled)
        {
            invocation.Proceed();

            return;
        }

        // Check if method has LogActivity attribute
        var logActivityAttribute = GetLogActivityAttribute(invocation.Method);

        if (logActivityAttribute == null)
        {
            invocation.Proceed();

            return;
        }

        // Check if method should be filtered out
        if (ShouldFilterMethod(invocation.Method))
        {
            invocation.Proceed();

            return;
        }

        var context = CreateExecutionContext(invocation, logActivityAttribute);

        try
        {
            // Start timing and logging
            context.StartTiming();

            // Log method start if configured
            if (_configuration.MinimumLogLevel <= ActivityLogLevel.Debug)
            {
                _ = Task.Run(async () => await activityLogService.LogMethodStartAsync(context));
            }

            // Execute the method
            invocation.Proceed();

            // Handle async methods
            if (invocation.Method.IsAsyncMethod())
            {
                HandleAsyncMethod(invocation, context);
            }
            else
            {
                // Handle synchronous methods
                context.ReturnValue = invocation.ReturnValue;
                context.StopTiming();

                // Log method completion
                _ = Task.Run(async () => await activityLogService.LogMethodCompletionAsync(context));
            }
        }
        catch (Exception ex)
        {
            context.Exception = ex;
            context.StopTiming();

            // Log method failure
            _ = Task.Run(async () => await activityLogService.LogMethodFailureAsync(context));

            // Re-throw the original exception
            throw;
        }
    }

    private LogActivityAttribute? GetLogActivityAttribute(MethodInfo method)
    {
        // Only check method-level attributes, not class-level
        return method.GetCustomAttribute<LogActivityAttribute>();
    }

    private bool ShouldFilterMethod(MethodInfo method)
    {
        var methodName    = method.Name;
        var namespaceName = method.DeclaringType?.Namespace ?? string.Empty;

        // Check excluded methods
        if (_configuration.Filtering.ExcludedMethods.Contains(methodName))
            return true;

        // Check namespace filtering
        if (_configuration.Filtering.UseWhitelistMode)
        {
            return !_configuration.Filtering.IncludedNamespaces.Any(ns =>
                                                                        namespaceName.StartsWith(ns,
                                                                                                 StringComparison
                                                                                                    .OrdinalIgnoreCase));
        }
        else
        {
            return _configuration.Filtering.ExcludedNamespaces.Any(ns =>
                                                                       namespaceName.StartsWith(ns,
                                                                                                StringComparison
                                                                                                   .OrdinalIgnoreCase));
        }
    }

    private MethodExecutionContext CreateExecutionContext(IInvocation invocation, LogActivityAttribute attribute)
    {
        var context = new MethodExecutionContext
        {
            MethodInfo          = invocation.Method,
            Target              = invocation.InvocationTarget,
            Arguments           = invocation.Arguments,
            ActivityName        = attribute.Name,
            ActivityDescription = attribute.Description,
            DescriptionFormat   = attribute.DescriptionFormat,
            CorrelationId       = contextProvider.GetCorrelationId(),
            UserContext         = contextProvider.GetUserContext(),
            HttpContext         = contextProvider.GetHttpContext()
        };

        return context;
    }

    private void HandleAsyncMethod(IInvocation invocation, MethodExecutionContext context)
    {
        var returnType = invocation.Method.ReturnType;

        if (returnType == typeof(Task))
        {
            invocation.ReturnValue = HandleTaskAsync((Task)invocation.ReturnValue, context);
        }
        else if (returnType.IsGenericType &&
                 returnType.GetGenericTypeDefinition() == typeof(Task<>))
        {
            var taskType = returnType.GetGenericArguments()[0];
            var method = typeof(ActivityLogInterceptor)
                        .GetMethod(nameof(HandleTaskWithResultAsync), BindingFlags.NonPublic | BindingFlags.Instance)!
                        .MakeGenericMethod(taskType);

            invocation.ReturnValue = method.Invoke(this,
                                                   new[]
                                                   {
                                                       invocation.ReturnValue, context
                                                   });
        }
        else if (returnType == typeof(ValueTask))
        {
            invocation.ReturnValue = HandleValueTaskAsync((ValueTask)invocation.ReturnValue, context);
        }
        else if (returnType.IsGenericType &&
                 returnType.GetGenericTypeDefinition() == typeof(ValueTask<>))
        {
            var taskType = returnType.GetGenericArguments()[0];
            var method = typeof(ActivityLogInterceptor)
                        .GetMethod(nameof(HandleValueTaskWithResultAsync),
                                   BindingFlags.NonPublic | BindingFlags.Instance)!
                        .MakeGenericMethod(taskType);

            invocation.ReturnValue = method.Invoke(this,
                                                   new[]
                                                   {
                                                       invocation.ReturnValue, context
                                                   });
        }
    }

    private async Task HandleTaskAsync(Task task, MethodExecutionContext context)
    {
        try
        {
            await task;
            context.StopTiming();
            await activityLogService.LogMethodCompletionAsync(context);
        }
        catch (Exception ex)
        {
            context.Exception = ex;
            context.StopTiming();
            await activityLogService.LogMethodFailureAsync(context);

            throw;
        }
    }

    private async Task<T> HandleTaskWithResultAsync<T>(Task<T> task, MethodExecutionContext context)
    {
        try
        {
            var result = await task;
            context.ReturnValue = result;
            context.StopTiming();
            await activityLogService.LogMethodCompletionAsync(context);

            return result;
        }
        catch (Exception ex)
        {
            context.Exception = ex;
            context.StopTiming();
            await activityLogService.LogMethodFailureAsync(context);

            throw;
        }
    }

    private async ValueTask HandleValueTaskAsync(ValueTask task, MethodExecutionContext context)
    {
        try
        {
            await task;
            context.StopTiming();
            await activityLogService.LogMethodCompletionAsync(context);
        }
        catch (Exception ex)
        {
            context.Exception = ex;
            context.StopTiming();
            await activityLogService.LogMethodFailureAsync(context);

            throw;
        }
    }

    private async ValueTask<T> HandleValueTaskWithResultAsync<T>(ValueTask<T> task, MethodExecutionContext context)
    {
        try
        {
            var result = await task;
            context.ReturnValue = result;
            context.StopTiming();
            await activityLogService.LogMethodCompletionAsync(context);

            return result;
        }
        catch (Exception ex)
        {
            context.Exception = ex;
            context.StopTiming();
            await activityLogService.LogMethodFailureAsync(context);

            throw;
        }
    }
}
