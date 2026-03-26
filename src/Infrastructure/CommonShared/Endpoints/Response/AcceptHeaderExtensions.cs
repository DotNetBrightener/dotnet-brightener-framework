using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace WebApp.CommonShared.Endpoints.Response;

/// <summary>
///     Extension methods for working with Accept headers in HTTP requests.
/// </summary>
public static class AcceptHeaderExtensions
{
    /// <summary>
    ///     Gets the accepted content types from the request, ordered by quality value (q).
    /// </summary>
    /// <param name="request">The HTTP request</param>
    /// <returns>Ordered list of accepted content types</returns>
    public static IReadOnlyList<AcceptMediaType> GetAcceptedContentTypes(this HttpRequest request)
    {
        var acceptHeader = request.Headers.Accept.ToString();

        if (string.IsNullOrEmpty(acceptHeader))
        {
            return new List<AcceptMediaType>
            {
                new("application/json", 1.0)
            }.AsReadOnly();
        }

        return ParseAcceptHeader(acceptHeader);
    }

    /// <summary>
    ///     Determines if the client accepts a specific content type.
    /// </summary>
    /// <param name="request">The HTTP request</param>
    /// <param name="contentType">The content type to check</param>
    /// <returns>True if the client accepts the content type</returns>
    public static bool AcceptsContentType(this HttpRequest request, string contentType)
    {
        var acceptedTypes = request.GetAcceptedContentTypes();

        return acceptedTypes.Any(t => t.Matches(contentType));
    }

    /// <summary>
    ///     Gets the best matching content type from the Accept header.
    /// </summary>
    /// <param name="request">The HTTP request</param>
    /// <param name="supportedTypes">The content types supported by the server</param>
    /// <returns>The best matching content type, or null if no match</returns>
    public static string? GetBestContentType(
        this HttpRequest request,
        params string[] supportedTypes)
    {
        var acceptedTypes = request.GetAcceptedContentTypes();

        foreach (var accepted in acceptedTypes)
        {
            var match = supportedTypes.FirstOrDefault(supported =>
                accepted.Matches(supported));

            if (match != null)
                return match;
        }

        return supportedTypes.FirstOrDefault();
    }

    /// <summary>
    ///     Parses the Accept header into ordered media types.
    /// </summary>
    private static IReadOnlyList<AcceptMediaType> ParseAcceptHeader(string acceptHeader)
    {
        var mediaTypes = new List<AcceptMediaType>();

        foreach (var segment in acceptHeader.Split(','))
        {
            var trimmed = segment.Trim();

            if (string.IsNullOrEmpty(trimmed))
                continue;

            var parts = trimmed.Split(';');
            var mediaType = parts[0].Trim();
            var quality = 1.0;

            foreach (var part in parts.Skip(1))
            {
                var parameter = part.Trim();

                if (parameter.StartsWith("q=", StringComparison.OrdinalIgnoreCase))
                {
                    var qualityValue = parameter[2..];

                    if (double.TryParse(qualityValue, out var q))
                        quality = q;
                }
            }

            mediaTypes.Add(new AcceptMediaType(mediaType, quality));
        }

        return mediaTypes.OrderByDescending(m => m.Quality).ToList().AsReadOnly();
    }
}

/// <summary>
///     Represents an accepted media type from the Accept header.
/// </summary>
public readonly record struct AcceptMediaType(string MediaType, double Quality)
{
    /// <summary>
    ///     Checks if this media type matches the specified content type.
    ///     Supports wildcards (*/* and type/*).
    /// </summary>
    /// <param name="contentType">The content type to match against</param>
    /// <returns>True if there's a match</returns>
    public bool Matches(string contentType)
    {
        if (string.IsNullOrEmpty(contentType))
            return false;

        var normalizedMediaType = MediaType.ToLowerInvariant();
        var normalizedContentType = contentType.ToLowerInvariant();

        // Exact match
        if (string.Equals(normalizedMediaType, normalizedContentType, StringComparison.OrdinalIgnoreCase))
            return true;

        // Wildcard match */*
        if (normalizedMediaType == "*/*")
            return true;

        // Type wildcard match (e.g., application/*)
        if (normalizedMediaType.EndsWith("/*"))
        {
            var typePrefix = normalizedMediaType[..^2];

            return normalizedContentType.StartsWith(typePrefix + "/");
        }

        return false;
    }

    /// <inheritdoc />
    public override string ToString() => $"{MediaType}; q={Quality}";
}
