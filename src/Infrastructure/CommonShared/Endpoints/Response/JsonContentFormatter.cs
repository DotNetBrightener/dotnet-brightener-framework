using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace WebApp.CommonShared.Endpoints.Response;

/// <summary>
///     Default JSON content formatter using System.Text.Json.
/// </summary>
public class JsonContentFormatter : IContentFormatter
{
    private readonly JsonSerializerOptions _serializerOptions;

    /// <summary>
    ///     Creates a new instance of JsonContentFormatter with default options.
    /// </summary>
    public JsonContentFormatter() : this(new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    })
    {
    }

    /// <summary>
    ///     Creates a new instance of JsonContentFormatter with custom options.
    /// </summary>
    /// <param name="serializerOptions">The JSON serializer options to use</param>
    public JsonContentFormatter(JsonSerializerOptions serializerOptions)
    {
        _serializerOptions = serializerOptions;
    }

    /// <inheritdoc />
    public IEnumerable<string> SupportedContentTypes => new[]
    {
        "application/json",
        "text/json",
        "application/*+json"
    };

    /// <inheritdoc />
    public bool CanFormat(string contentType)
    {
        if (string.IsNullOrEmpty(contentType))
            return false;

        var normalizedContentType = contentType.ToLowerInvariant().Trim();

        return SupportedContentTypes.Any(supported =>
            string.Equals(supported, normalizedContentType, StringComparison.OrdinalIgnoreCase) ||
            (supported.EndsWith("*") && normalizedContentType.StartsWith(supported[..^1])) ||
            normalizedContentType.Contains("json"));
    }

    /// <inheritdoc />
    public async Task FormatAsync<T>(HttpContext context, T? value, CancellationToken cancellationToken = default)
    {
        context.Response.ContentType = "application/json";

        if (value is null)
        {
            context.Response.ContentLength = 0;

            return;
        }

        await using var memoryStream = new MemoryStream();
        await JsonSerializer.SerializeAsync(memoryStream, value, _serializerOptions, cancellationToken);

        context.Response.ContentLength = memoryStream.Length;
        memoryStream.Position = 0;
        await memoryStream.CopyToAsync(context.Response.Body, cancellationToken);
    }
}
