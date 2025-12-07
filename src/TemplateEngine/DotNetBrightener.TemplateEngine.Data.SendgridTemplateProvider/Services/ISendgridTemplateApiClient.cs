using DotNetBrightener.TemplateEngine.Data.SendgridTemplateProvider.Models;

namespace DotNetBrightener.TemplateEngine.Data.SendgridTemplateProvider.Services;

/// <summary>
///     Client interface for interacting with the Sendgrid Templates API.
/// </summary>
public interface ISendgridTemplateApiClient
{
    /// <summary>
    ///     Retrieves all dynamic templates from Sendgrid.
    /// </summary>
    /// <returns>A list of templates with their IDs and names.</returns>
    Task<IReadOnlyList<SendgridTemplateInfo>> GetAllTemplatesAsync();

    /// <summary>
    ///     Gets a template by its ID, including version information.
    /// </summary>
    /// <param name="templateId">The Sendgrid template ID.</param>
    /// <returns>The template details including active version content, or null if not found.</returns>
    Task<SendgridTemplateDetails> GetTemplateAsync(string templateId);

    /// <summary>
    ///     Creates a new dynamic template in Sendgrid.
    /// </summary>
    /// <param name="name">The name of the template to create.</param>
    /// <returns>The created template information.</returns>
    Task<SendgridTemplateInfo> CreateTemplateAsync(string name);
}

