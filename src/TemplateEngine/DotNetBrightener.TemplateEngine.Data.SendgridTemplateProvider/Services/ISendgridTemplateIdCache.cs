namespace DotNetBrightener.TemplateEngine.Data.SendgridTemplateProvider.Services;

/// <summary>
///     Cache for storing and retrieving Sendgrid template IDs by template type name.
/// </summary>
public interface ISendgridTemplateIdCache
{
    /// <summary>
    ///     Attempts to get a template ID for the given template type.
    /// </summary>
    /// <param name="templateTypeName">The full type name of the template (e.g., "MyApp.Templates.WelcomeEmail").</param>
    /// <param name="templateId">The Sendgrid template ID if found.</param>
    /// <returns>True if the template ID was found; otherwise false.</returns>
    bool TryGetTemplateId(string templateTypeName, out string templateId);

    /// <summary>
    ///     Sets the template ID for the given template type.
    /// </summary>
    /// <param name="templateTypeName">The full type name of the template.</param>
    /// <param name="templateId">The Sendgrid template ID.</param>
    void SetTemplateId(string templateTypeName, string templateId);

    /// <summary>
    ///     Clears all cached template IDs.
    /// </summary>
    void Clear();
}

