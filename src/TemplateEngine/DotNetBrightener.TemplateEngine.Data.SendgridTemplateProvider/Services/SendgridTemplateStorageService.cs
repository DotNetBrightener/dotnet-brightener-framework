using DotNetBrightener.TemplateEngine.Data.Models;
using DotNetBrightener.TemplateEngine.Data.Services;
using Microsoft.Extensions.Logging;

namespace DotNetBrightener.TemplateEngine.Data.SendgridTemplateProvider.Services;

/// <summary>
///     Sendgrid implementation of the template storage service.
///     Loads template content from Sendgrid dynamic templates.
///     Saving is not supported as templates are managed externally in the Sendgrid UI.
/// </summary>
public class SendgridTemplateStorageService(
    ISendgridTemplateIdCache                templateIdCache,
    ISendgridTemplateApiClient              apiClient,
    ILogger<SendgridTemplateStorageService> logger)
    : ITemplateStorageService
{
    /// <inheritdoc />
    /// <exception cref="NotSupportedException">
    ///     Always thrown. Template content is managed externally in the Sendgrid UI.
    /// </exception>
    public Task SaveTemplateAsync(string templateType, TemplateModelDto content)
    {
        throw new NotSupportedException(
            "Saving templates is not supported by the Sendgrid provider. " +
            "Template content should be managed directly in the Sendgrid UI or via the Sendgrid API.");
    }

    /// <inheritdoc />
    public async Task<TemplateModelDto> LoadTemplateAsync(string templateModelType)
    {
        logger.LogDebug("Loading template for type {TemplateType}", templateModelType);

        // Look up the template ID from the cache
        if (!templateIdCache.TryGetTemplateId(templateModelType, out var templateId))
        {
            logger.LogWarning("Template ID not found in cache for type {TemplateType}. " +
                              "Ensure the template was registered during startup.",
                              templateModelType);

            throw new TemplateNotFoundException(
                $"Template not found for type '{templateModelType}'. " +
                "The template may not have been registered or may not exist in Sendgrid.");
        }

        // Fetch the template details from Sendgrid
        var templateDetails = await apiClient.GetTemplateAsync(templateId);

        if (templateDetails == null)
        {
            logger.LogError("Template {TemplateId} for type {TemplateType} was not found in Sendgrid",
                            templateId,
                            templateModelType);

            throw new TemplateNotFoundException(
                $"Template with ID '{templateId}' for type '{templateModelType}' was not found in Sendgrid.");
        }

        // Get the active version content
        var activeVersion = templateDetails.ActiveVersion;

        if (activeVersion == null)
        {
            logger.LogWarning("Template {TemplateId} for type {TemplateType} has no active version",
                              templateId,
                              templateModelType);

            // Return empty content if no active version exists
            return new TemplateModelDto
            {
                TemplateType    = templateModelType,
                TemplateTitle   = templateDetails.Name,
                TemplateContent = string.Empty
            };
        }

        return new TemplateModelDto
        {
            TemplateType    = templateModelType,
            TemplateTitle   = activeVersion.Subject,
            TemplateContent = activeVersion.HtmlContent
        };
    }
}

/// <summary>
///     Exception thrown when a template is not found.
/// </summary>
public class TemplateNotFoundException : Exception
{
    public TemplateNotFoundException(string message) : base(message)
    {
    }

    public TemplateNotFoundException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

