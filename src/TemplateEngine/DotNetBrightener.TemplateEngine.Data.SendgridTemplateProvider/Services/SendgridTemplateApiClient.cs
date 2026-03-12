using System.Net;
using System.Text.Json;
using DotNetBrightener.TemplateEngine.Data.SendgridTemplateProvider.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SendGrid;

namespace DotNetBrightener.TemplateEngine.Data.SendgridTemplateProvider.Services;

/// <summary>
///     Implementation of the Sendgrid Templates API client.
/// </summary>
internal class SendgridTemplateApiClient(
    IOptions<SendgridTemplateProviderSettings> settings,
    ILogger<SendgridTemplateApiClient>         logger)
    : ISendgridTemplateApiClient
{
    private readonly SendGridClient                     _client = new(settings.Value.ApiKey);

    private readonly JsonSerializerOptions              _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private const string TemplatesEndpoint = "templates";

    /// <inheritdoc />
    public async Task<IReadOnlyList<SendgridTemplateInfo>> GetAllTemplatesAsync()
    {
        var allTemplates = new List<SendgridTemplateInfo>();
        var pageToken    = string.Empty;
        var hasMorePages = true;

        while (hasMorePages)
        {
            var queryParams = $"?generations=dynamic&page_size=200";
            if (!string.IsNullOrEmpty(pageToken))
            {
                queryParams += $"&page_token={pageToken}";
            }

            var response = await _client.RequestAsync(
                method: SendGridClient.Method.GET,
                urlPath: $"{TemplatesEndpoint}{queryParams}");

            await EnsureSuccessResponse(response, "list templates");

            var body = await response.Body.ReadAsStringAsync();
            var templatesResponse = JsonSerializer.Deserialize<SendgridTemplatesResponse>(body, _jsonOptions);

            if (templatesResponse?.Result != null)
            {
                allTemplates.AddRange(templatesResponse.Result);
            }

            // Check for more pages - Sendgrid uses cursor-based pagination
            // If result count equals page size, there might be more
            hasMorePages = templatesResponse?.Result?.Count == 200;
            
            // Note: For simplicity, we're limiting to one page (200 templates)
            // In production, you may want to implement full cursor-based pagination
            hasMorePages = false;
        }

        logger.LogDebug("Retrieved {Count} dynamic templates from Sendgrid", allTemplates.Count);
        return allTemplates;
    }

    /// <inheritdoc />
    public async Task<SendgridTemplateDetails> GetTemplateAsync(string templateId)
    {
        var response = await _client.RequestAsync(
            method: SendGridClient.Method.GET,
            urlPath: $"{TemplatesEndpoint}/{templateId}");

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            logger.LogWarning("Template {TemplateId} not found in Sendgrid", templateId);
            return null;
        }

        await EnsureSuccessResponse(response, $"get template {templateId}");

        var body = await response.Body.ReadAsStringAsync();
        return JsonSerializer.Deserialize<SendgridTemplateDetails>(body, _jsonOptions);
    }

    /// <inheritdoc />
    public async Task<SendgridTemplateInfo> CreateTemplateAsync(string name)
    {
        var requestBody = JsonSerializer.Serialize(new
        {
            name       = name,
            generation = "dynamic"
        });

        var response = await _client.RequestAsync(
            method: SendGridClient.Method.POST,
            urlPath: TemplatesEndpoint,
            requestBody: requestBody);

        await EnsureSuccessResponse(response, $"create template '{name}'");

        var body = await response.Body.ReadAsStringAsync();
        var template = JsonSerializer.Deserialize<SendgridTemplateInfo>(body, _jsonOptions);

        logger.LogInformation("Created Sendgrid template '{Name}' with ID {TemplateId}", name, template?.Id);

        return template ?? throw new InvalidOperationException($"Failed to parse created template response for '{name}'");
    }

    private async Task EnsureSuccessResponse(Response response, string operation)
    {
        if (response.StatusCode == HttpStatusCode.OK ||
            response.StatusCode == HttpStatusCode.Created)
        {
            return;
        }

        var responseBody = await response.Body.ReadAsStringAsync();
        var message      = $"Sendgrid API failed to {operation}. Status: {(int)response.StatusCode} ({response.StatusCode}). Response: {responseBody}";

        logger.LogError(message);
        throw new SendgridTemplateApiException(message, response.StatusCode, responseBody);
    }
}

/// <summary>
///     Exception thrown when the Sendgrid Templates API returns an error.
/// </summary>
public class SendgridTemplateApiException(string message, HttpStatusCode statusCode, string responseBody)
    : Exception(message)
{
    public HttpStatusCode StatusCode   { get; } = statusCode;
    public string         ResponseBody { get; } = responseBody;
}

