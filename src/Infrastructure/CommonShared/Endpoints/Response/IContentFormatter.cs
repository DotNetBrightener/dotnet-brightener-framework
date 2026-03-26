using Microsoft.AspNetCore.Http;

namespace WebApp.CommonShared.Endpoints.Response;

/// <summary>
///     Defines a contract for formatting response content based on content type negotiation.
///     Implementations handle different content types (JSON, XML, etc.) for API responses.
/// </summary>
public interface IContentFormatter
{
    /// <summary>
    ///     Gets the supported content types for this formatter.
    ///     Example: "application/json", "text/json"
    /// </summary>
    IEnumerable<string> SupportedContentTypes { get; }

    /// <summary>
    ///     Determines whether this formatter can handle the specified content type.
    /// </summary>
    /// <param name="contentType">The content type to check</param>
    /// <returns>True if this formatter can handle the content type</returns>
    bool CanFormat(string contentType);

    /// <summary>
    ///     Formats and writes the response to the HTTP context.
    /// </summary>
    /// <typeparam name="T">The type of value to format</typeparam>
    /// <param name="context">The HTTP context to write to</param>
    /// <param name="value">The value to format and write</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A task representing the async operation</returns>
    Task FormatAsync<T>(HttpContext context, T? value, CancellationToken cancellationToken = default);
}
