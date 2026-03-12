using Microsoft.AspNetCore.Builder;

namespace DotNetBrightener.DataAccess.EF.TransactionManagement;

/// <summary>
/// Extension methods for registering transaction middleware.
/// </summary>
public static class TransactionMiddlewareExtensions
{
    /// <summary>
    /// Adds the transaction middleware to the application pipeline.
    /// </summary>
    /// <param name="builder">The application builder.</param>
    /// <returns>The application builder for chaining.</returns>
    public static IApplicationBuilder UseTransactionManagement(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<TransactionMiddleware>();
    }

    /// <summary>
    /// Adds the transaction middleware to the application pipeline with custom options.
    /// </summary>
    /// <param name="builder">The application builder.</param>
    /// <param name="configureOptions">Action to configure transaction middleware options.</param>
    /// <returns>The application builder for chaining.</returns>
    public static IApplicationBuilder UseTransactionManagement(
        this IApplicationBuilder             builder,
        Action<TransactionMiddlewareOptions> configureOptions)
    {
        var options = new TransactionMiddlewareOptions();
        configureOptions(options);

        return builder.UseMiddleware<TransactionMiddleware>(Microsoft.Extensions.Options.Options.Create(options));
    }
}
