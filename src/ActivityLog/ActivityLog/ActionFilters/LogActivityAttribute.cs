using ActivityLog.Configuration;
using ActivityLog.Models;
using ActivityLog.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Reflection;

namespace ActivityLog.ActionFilters;

[AttributeUsage(validOn: AttributeTargets.Method)]
public class LogActivityAttribute(string name, string? descriptionFormat = null) : Attribute, IActionFilter
{
    public LogActivityAttribute(string name)
        : this(name, null)
    {
    }

    public string Name { get; set; } = name;

    public string Description { get; set; }

    public string TargetEntity { get; set; }

    public string? DescriptionFormat { get; set; } = descriptionFormat;

    // Key for storing context in HttpContext.Items
    private const string ContextKey = "ActivityLog_ExecutionContext";

    /// <summary>
    /// Called before the action method is executed (IActionFilter implementation)
    /// </summary>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        var serviceProvider = context.HttpContext.RequestServices;
        var configuration   = serviceProvider.GetService<IOptions<ActivityLogConfiguration>>()?.Value;

        // Check if logging is enabled
        if (configuration?.IsEnabled != true)
        {
            return;
        }

        var activityLogService = serviceProvider.GetService<IActivityLogService>();
        var contextProvider    = serviceProvider.GetService<IActivityLogContextProvider>();

        if (activityLogService == null ||
            contextProvider == null)
        {
            return;
        }

        // Check if method should be filtered out
        if (context.ActionDescriptor is ControllerActionDescriptor controllerActionDescriptor)
        {
            if (ShouldFilterMethod(controllerActionDescriptor.MethodInfo, configuration))
            {
                return;
            }
        }

        // Create execution context for the controller action
        var executionContext = CreateControllerExecutionContext(context, contextProvider);

        // Store context in HttpContext.Items for use in OnActionExecuted
        context.HttpContext.Items[ContextKey] = executionContext;

        // Set up context for metadata collection
        var previousContext = ActivityLogContextAccessor.CurrentContext;
        ActivityLogContextAccessor.CurrentContext = executionContext;

        // Store previous context to restore later
        context.HttpContext.Items[ContextKey + "_Previous"] = previousContext;
    }

    /// <summary>
    /// Called after the action method is executed (IActionFilter implementation)
    /// </summary>
    public void OnActionExecuted(ActionExecutedContext context)
    {
        // Retrieve stored context
        if (context.HttpContext.Items[ContextKey] is not MethodExecutionContext executionContext)
        {
            return;
        }

        var serviceProvider    = context.HttpContext.RequestServices;
        var activityLogService = serviceProvider.GetService<IActivityLogService>();

        if (activityLogService == null)
        {
            return;
        }

        try
        {
            // Stop timing and capture results
            executionContext.StopTiming();

            // Capture return value from action result
            CaptureActionResult(context, executionContext);

            // Handle exceptions
            if (context.Exception != null)
            {
                executionContext.Exception = context.Exception;

                // Log method failure
                _ = Task.Run(async () => await activityLogService.LogMethodFailureAsync(executionContext));
            }
            else
            {
                // Log method completion
                _ = Task.Run(async () => await activityLogService.LogMethodCompletionAsync(executionContext));
            }
        }
        finally
        {
            // Restore previous context
            var previousContext = context.HttpContext.Items[ContextKey + "_Previous"] as MethodExecutionContext;
            ActivityLogContextAccessor.CurrentContext = previousContext;

            // Clean up HttpContext.Items
            context.HttpContext.Items.Remove(ContextKey);
            context.HttpContext.Items.Remove(ContextKey + "_Previous");
        }
    }

    /// <summary>
    /// Creates execution context for controller actions
    /// </summary>
    private MethodExecutionContext CreateControllerExecutionContext(ActionExecutingContext      context,
                                                                    IActivityLogContextProvider contextProvider)
    {
        var         actionDescriptor = context.ActionDescriptor;
        MethodInfo? methodInfo       = null;

        // Get MethodInfo from ControllerActionDescriptor
        if (actionDescriptor is ControllerActionDescriptor controllerActionDescriptor)
        {
            methodInfo = controllerActionDescriptor.MethodInfo;
        }

        // Fallback if we can't get MethodInfo (shouldn't happen in normal scenarios)
        if (methodInfo == null)
        {
            // Create a dummy MethodInfo for the action
            methodInfo = typeof(LogActivityAttribute).GetMethod(nameof(OnActionExecuting))!;
        }

        // Create named arguments dictionary from action parameters
        var namedArguments = CreateNamedArgumentsFromActionContext(context);

        var executionContext = new MethodExecutionContext
        {
            MethodInfo          = methodInfo,
            Arguments           = namedArguments,
            ActivityName        = Name,
            ActivityDescription = Description,
            DescriptionFormat   = DescriptionFormat,
            TargetEntity        = TargetEntity,
            CorrelationId       = contextProvider.GetCorrelationId(),
            UserContext         = contextProvider.GetUserContext(),
            HttpContext         = contextProvider.GetHttpContext()
        };

        executionContext.StartTiming();

        return executionContext;
    }

    /// <summary>
    /// Creates named arguments dictionary from action executing context
    /// </summary>
    private Dictionary<string, object?> CreateNamedArgumentsFromActionContext(ActionExecutingContext context)
    {
        var namedArguments = new Dictionary<string, object?>();

        foreach (var parameter in context.ActionArguments)
        {
            namedArguments[parameter.Key] = parameter.Value;
        }

        return namedArguments;
    }

    /// <summary>
    /// Captures the action result for logging
    /// </summary>
    private void CaptureActionResult(ActionExecutedContext context, MethodExecutionContext executionContext)
    {
        if (context.Result == null)
        {
            return;
        }

        // Handle different types of action results
        switch (context.Result)
        {
            case ObjectResult objectResult:
                executionContext.ReturnValue = objectResult.Value;

                break;
            case JsonResult jsonResult:
                executionContext.ReturnValue = jsonResult.Value;

                break;
            case ContentResult contentResult:
                executionContext.ReturnValue = contentResult.Content;

                break;
            case StatusCodeResult statusCodeResult:
                executionContext.ReturnValue = new
                {
                    StatusCode = statusCodeResult.StatusCode
                };

                break;
            case RedirectResult redirectResult:
                executionContext.ReturnValue = new
                {
                    Url       = redirectResult.Url,
                    Permanent = redirectResult.Permanent
                };

                break;
            case RedirectToActionResult redirectToActionResult:
                executionContext.ReturnValue = new
                {
                    ActionName     = redirectToActionResult.ActionName,
                    ControllerName = redirectToActionResult.ControllerName,
                    RouteValues    = redirectToActionResult.RouteValues
                };

                break;
            default:
                // For other result types, capture the type name
                executionContext.ReturnValue = new
                {
                    ResultType = context.Result.GetType().Name
                };

                break;
        }
    }

    /// <summary>
    /// Checks if the method should be filtered out based on configuration
    /// </summary>
    private bool ShouldFilterMethod(MethodInfo method, ActivityLogConfiguration configuration)
    {
        if (configuration.Filtering == null)
        {
            return false;
        }

        var fullMethodName = $"{method.DeclaringType?.FullName}.{method.Name}";
        var namespaceName  = method.DeclaringType?.Namespace ?? string.Empty;

        // Check excluded namespaces
        if (configuration.Filtering.ExcludedNamespaces?.Any(ns =>
                                                                namespaceName.StartsWith(ns,
                                                                                         StringComparison
                                                                                            .OrdinalIgnoreCase)) ==
            true)
        {
            return true;
        }

        // Check excluded methods
        if (configuration.Filtering.ExcludedMethods?.Any(methodName =>
                                                             method.Name.Equals(methodName,
                                                                                StringComparison.OrdinalIgnoreCase) ||
                                                             fullMethodName.Equals(methodName,
                                                                                   StringComparison
                                                                                      .OrdinalIgnoreCase)) ==
            true)
        {
            return true;
        }

        return false;
    }
}