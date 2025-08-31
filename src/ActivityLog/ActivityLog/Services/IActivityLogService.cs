using ActivityLog.Models;

namespace ActivityLog.Services;

/// <summary>
/// Service for logging method execution activities
/// </summary>
public interface IActivityLogService
{
    /// <summary>
    /// Logs the start of a method execution
    /// </summary>
    /// <param name="context">The method execution context</param>
    /// <returns>A task representing the async operation</returns>
    Task<LoggingResult> LogMethodStartAsync(MethodExecutionContext context);

    /// <summary>
    /// Logs the successful completion of a method execution
    /// </summary>
    /// <param name="context">The method execution context</param>
    /// <returns>A task representing the async operation</returns>
    Task<LoggingResult> LogMethodCompletionAsync(MethodExecutionContext context);

    /// <summary>
    /// Logs the failure of a method execution
    /// </summary>
    /// <param name="context">The method execution context</param>
    /// <returns>A task representing the async operation</returns>
    Task<LoggingResult> LogMethodFailureAsync(MethodExecutionContext context);

    /// <summary>
    /// Logs a complete method execution (start and end in one call)
    /// </summary>
    /// <param name="context">The method execution context</param>
    /// <returns>A task representing the async operation</returns>
    Task<LoggingResult> LogMethodExecutionAsync(MethodExecutionContext context);

    /// <summary>
    /// Flushes any pending log entries
    /// </summary>
    /// <returns>A task representing the async operation</returns>
    Task FlushAsync();
}
