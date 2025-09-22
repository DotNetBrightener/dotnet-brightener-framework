using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DotNetBrightener.DataAccess.EF.TransactionManagement;

/// <summary>
/// Configuration options for transaction middleware behavior.
/// </summary>
public class TransactionMiddlewareOptions
{
    /// <summary>
    ///     Gets or sets a value indicating whether automatic transactions are enabled.
    ///     Default is true.
    /// </summary>
    public bool EnableAutomaticTransactions { get; set; } = true;

    /// <summary>
    ///     Gets or sets the transaction timeout duration.
    ///     Default is 5 minutes.
    /// </summary>
    public TimeSpan TransactionTimeout { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    ///     Gets or sets a value indicating whether to log transaction lifecycle events.
    ///     Default is true.
    /// </summary>
    public bool LogTransactionLifecycle { get; set; } = true;

    /// <summary>
    ///     Gets or sets HTTP methods that should skip automatic transaction management.
    ///     Default includes GET, HEAD, OPTIONS.
    /// </summary>
    public HashSet<string> SkipTransactionForMethods { get; set; } = new(StringComparer.OrdinalIgnoreCase)
    {
        "GET", "HEAD", "OPTIONS"
    };

    /// <summary>
    /// Gets or sets path patterns that should skip automatic transaction management.
    /// Supports wildcards (*).
    /// </summary>
    public HashSet<string> SkipTransactionForPaths { get; set; } = new(StringComparer.OrdinalIgnoreCase)
    {
        "/health", "/metrics", "/swagger*", "/api/health*"
    };
}

/// <summary>
/// Middleware that automatically manages database transactions for HTTP requests.
/// Begins a transaction at the start of each request and commits or rolls back based on the outcome.
/// </summary>
public class TransactionMiddleware
{
    private readonly RequestDelegate                _next;
    private readonly ILogger<TransactionMiddleware> _logger;
    private readonly TransactionMiddlewareOptions   _options;

    public TransactionMiddleware(RequestDelegate                        next,
                                 ILogger<TransactionMiddleware>         logger,
                                 IOptions<TransactionMiddlewareOptions> options)
    {
        _next    = next ?? throw new ArgumentNullException(nameof(next));
        _logger  = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options?.Value ?? new TransactionMiddlewareOptions();
    }

    /// <summary>
    /// Invokes the middleware to handle the HTTP request with automatic transaction management.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <param name="transactionManager">The request-scoped transaction manager.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task InvokeAsync(HttpContext context, ITransactionManager transactionManager)
    {
        // Check if we should skip transaction management for this request
        if (!_options.EnableAutomaticTransactions ||
            ShouldSkipTransaction(context))
        {
            if (_options.LogTransactionLifecycle)
            {
                _logger.LogDebug("Skipping automatic transaction management for {Method} {Path}",
                                 context.Request.Method,
                                 context.Request.Path);
            }

            await _next(context);

            return;
        }

        var requestId = context.TraceIdentifier;
        var method    = context.Request.Method;
        var path      = context.Request.Path;

        try
        {
            if (_options.LogTransactionLifecycle)
            {
                _logger.LogDebug("Starting automatic transaction management for request {RequestId} - {Method} {Path}",
                                 requestId,
                                 method,
                                 path);
            }

            // Begin transaction for the request
            await transactionManager.BeginTransactionAsync();

            if (_options.LogTransactionLifecycle)
            {
                _logger.LogDebug("Transaction started for request {RequestId}", requestId);
            }

            // Execute the rest of the pipeline
            await _next(context);

            // If we reach here without exceptions and the response is successful,
            // commit the transaction
            if (IsSuccessStatusCode(context.Response.StatusCode))
            {
                await transactionManager.CommitTransactionAsync();

                if (_options.LogTransactionLifecycle)
                {
                    _logger.LogDebug("Transaction committed successfully for request {RequestId}", requestId);
                }
            }
            else
            {
                // Non-success status codes should rollback the transaction
                await transactionManager.RollbackTransactionAsync();

                if (_options.LogTransactionLifecycle)
                {
                    _logger.LogWarning("Transaction rolled back due to non-success status code {StatusCode} for request {RequestId}",
                                       context.Response.StatusCode,
                                       requestId);
                }
            }
        }
        catch (Exception ex)
        {
            // Any exception should trigger a rollback
            _logger.LogError(ex,
                             "Exception occurred during request {RequestId} - {Method} {Path}. Rolling back transaction.",
                             requestId,
                             method,
                             path);

            try
            {
                await transactionManager.RollbackTransactionAsync();

                if (_options.LogTransactionLifecycle)
                {
                    _logger.LogDebug("Transaction rolled back successfully for request {RequestId} due to exception", requestId);
                }
            }
            catch (Exception rollbackEx)
            {
                _logger.LogError(rollbackEx, "Failed to rollback transaction for request {RequestId}", requestId);
                // Don't mask the original exception
            }

            // Re-throw the original exception
            throw;
        }
    }

    /// <summary>
    ///     Determines whether transaction management should be skipped for the current request.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <returns><c>True</c> if transaction management should be skipped; otherwise, <c>false</c>.</returns>
    private bool ShouldSkipTransaction(HttpContext context)
    {
        var method = context.Request.Method;
        var path   = context.Request.Path.Value ?? string.Empty;

        // Skip for certain HTTP methods (typically read-only operations)
        if (_options.SkipTransactionForMethods.Contains(method))
        {
            return true;
        }

        // Skip for certain paths
        foreach (var skipPath in _options.SkipTransactionForPaths)
        {
            if (IsPathMatch(path, skipPath))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    ///     Checks if a path matches a pattern (supports wildcards).
    /// </summary>
    /// <param name="path">The actual path.</param>
    /// <param name="pattern">The pattern to match against (supports * wildcard).</param>
    /// <returns>True if the path matches the pattern; otherwise, false.</returns>
    private static bool IsPathMatch(string path, string pattern)
    {
        if (pattern.Contains('*'))
        {
            var regexPattern = "^" + pattern.Replace("*", ".*") + "$";

            return System.Text.RegularExpressions.Regex.IsMatch(path,
                                                                regexPattern,
                                                                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }

        return string.Equals(path, pattern, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    ///     Determines if an HTTP status code represents a successful response.
    /// </summary>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <returns>True if the status code is in the 200-299 range; otherwise, false.</returns>
    private static bool IsSuccessStatusCode(int statusCode)
    {
        return statusCode is >= 200 and <= 299;
    }
}
