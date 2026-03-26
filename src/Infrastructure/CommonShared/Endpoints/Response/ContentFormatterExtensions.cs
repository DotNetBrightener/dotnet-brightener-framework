using System.Net.Mime;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;

namespace WebApp.CommonShared.Endpoints.Response;

/// <summary>
///     Extension methods for content formatting and response handling.
/// </summary>
public static class ContentFormatterExtensions
{
    /// <summary>
    ///     Formats the response using content negotiation based on Accept header.
    ///     Falls back to JSON if no matching formatter is found.
    /// </summary>
    /// <typeparam name="T">The type of value to format</typeparam>
    /// <param name="context">The HTTP context</param>
    /// <param name="value">The value to format</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <example>
    /// await context.FormatResponse(user);
    /// </example>
    public static async Task FormatResponse<T>(
        this HttpContext context,
        T? value,
        CancellationToken cancellationToken = default)
    {
        var formatters = context.RequestServices.GetServices<IContentFormatter>().ToList();

        if (formatters.Count == 0)
        {
            // Fallback to default JSON if no formatters registered
            var defaultFormatter = new JsonContentFormatter();
            await defaultFormatter.FormatAsync(context, value, cancellationToken);

            return;
        }

        var acceptHeader = context.Request.Headers.Accept.ToString();
        var formatter = GetFormatterForAccept(formatters, acceptHeader) ?? formatters.First();

        await formatter.FormatAsync(context, value, cancellationToken);
    }

    /// <summary>
    ///     Creates an IResult that formats the response using content negotiation.
    /// </summary>
    /// <typeparam name="T">The type of value to format</typeparam>
    /// <param name="value">The value to format</param>
    /// <returns>An IResult that performs content formatting when executed</returns>
    public static IResult Formatted<T>(T? value)
    {
        return new FormattedResult<T>(value);
    }

    /// <summary>
    ///     Gets the best matching formatter for the Accept header.
    /// </summary>
    private static IContentFormatter? GetFormatterForAccept(
        IEnumerable<IContentFormatter> formatters,
        string acceptHeader)
    {
        if (string.IsNullOrEmpty(acceptHeader))
            return null;

        var acceptValues = acceptHeader.Split(',')
                                        .Select(h => h.Trim())
                                        .Where(h => !string.IsNullOrEmpty(h));

        foreach (var accept in acceptValues)
        {
            var formatter = formatters.FirstOrDefault(f => f.CanFormat(accept));

            if (formatter != null)
                return formatter;
        }

        return null;
    }

    /// <summary>
    ///     Extension method to add content formatters to the service collection.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configure">Optional action to configure formatters</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddContentFormatters(
        this IServiceCollection services,
        Action<ContentFormatterOptions>? configure = null)
    {
        var options = new ContentFormatterOptions();
        configure?.Invoke(options);

        // Always register JSON formatter as default
        services.AddSingleton<IContentFormatter, JsonContentFormatter>();

        // Register custom formatters
        foreach (var formatterType in options.AdditionalFormatters)
        {
            services.AddSingleton(typeof(IContentFormatter), formatterType);
        }

        return services;
    }
}

/// <summary>
///     Options for configuring content formatters.
/// </summary>
public class ContentFormatterOptions
{
    /// <summary>
    ///     Additional formatter types to register.
    /// </summary>
    public List<Type> AdditionalFormatters { get; } = new();

    /// <summary>
    ///     Adds a custom content formatter type.
    /// </summary>
    /// <typeparam name="TFormatter">The formatter type</typeparam>
    /// <returns>The options for chaining</returns>
    public ContentFormatterOptions AddFormatter<TFormatter>() where TFormatter : IContentFormatter
    {
        AdditionalFormatters.Add(typeof(TFormatter));

        return this;
    }
}

/// <summary>
///     An IResult implementation that performs content formatting.
/// </summary>
internal class FormattedResult<T>(T? Value) : IResult
{
    public async Task ExecuteAsync(HttpContext httpContext)
    {
        await httpContext.FormatResponse(Value);
    }
}
