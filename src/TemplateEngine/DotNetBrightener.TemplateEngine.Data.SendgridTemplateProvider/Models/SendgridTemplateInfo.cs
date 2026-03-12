using System.Text.Json.Serialization;

namespace DotNetBrightener.TemplateEngine.Data.SendgridTemplateProvider.Models;

/// <summary>
///     Represents basic information about a Sendgrid template.
/// </summary>
public class SendgridTemplateInfo
{
    /// <summary>
    ///     The unique identifier of the template (e.g., "d-xxxxx").
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    ///     The name of the template.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    ///     The generation of the template ("legacy" or "dynamic").
    /// </summary>
    [JsonPropertyName("generation")]
    public string Generation { get; set; } = string.Empty;
}

/// <summary>
///     Represents detailed information about a Sendgrid template, including versions.
/// </summary>
public class SendgridTemplateDetails : SendgridTemplateInfo
{
    /// <summary>
    ///     The versions of the template.
    /// </summary>
    [JsonPropertyName("versions")]
    public List<SendgridTemplateVersion> Versions { get; set; } = new();

    /// <summary>
    ///     Gets the active version of the template, if any.
    /// </summary>
    public SendgridTemplateVersion ActiveVersion =>
        Versions.FirstOrDefault(v => v.Active == 1);
}

/// <summary>
///     Represents a version of a Sendgrid template.
/// </summary>
public class SendgridTemplateVersion
{
    /// <summary>
    ///     The unique identifier of the version.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    ///     The name of the version.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    ///     Whether this version is active (1) or not (0).
    /// </summary>
    [JsonPropertyName("active")]
    public int Active { get; set; }

    /// <summary>
    ///     The HTML content of the template.
    /// </summary>
    [JsonPropertyName("html_content")]
    public string HtmlContent { get; set; } = string.Empty;

    /// <summary>
    ///     The plain text content of the template.
    /// </summary>
    [JsonPropertyName("plain_content")]
    public string PlainContent { get; set; } = string.Empty;

    /// <summary>
    ///     The subject line of the template.
    /// </summary>
    [JsonPropertyName("subject")]
    public string Subject { get; set; } = string.Empty;
}

/// <summary>
///     Response from the Sendgrid List Templates API.
/// </summary>
internal class SendgridTemplatesResponse
{
    [JsonPropertyName("result")]
    public List<SendgridTemplateInfo> Result { get; set; } = new();

    [JsonPropertyName("_metadata")]
    public SendgridPaginationMetadata Metadata { get; set; }
}

/// <summary>
///     Pagination metadata from Sendgrid API responses.
/// </summary>
internal class SendgridPaginationMetadata
{
    [JsonPropertyName("count")]
    public int Count { get; set; }

    [JsonPropertyName("self")]
    public string Self { get; set; } = string.Empty;

    [JsonPropertyName("next")]
    public string Next { get; set; } = string.Empty;
}

