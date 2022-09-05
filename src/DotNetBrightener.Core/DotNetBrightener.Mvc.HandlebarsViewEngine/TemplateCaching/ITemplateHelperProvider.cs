using HandlebarsDotNet;

namespace DotNetBrightener.Mvc.HandlebarsViewEngine.TemplateCaching;

/// <summary>
/// Represents a helper for template service
/// </summary>
public interface ITemplateHelperProvider
{
    /// <summary>
    /// Indicates the prefix for the helper to use in the template
    /// </summary>
    string TemplateSyntaxPrefix { get; }

    /// <summary>
    /// The instruction of using the template helper
    /// </summary>
    string UsageHint { get; }

    /// <summary>
    /// The description of the template helper
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Indicates the helper template should be registered as block template
    /// </summary>
    bool IsBlockTemplate { get; }

    /// <summary>
    /// Called by the template service to render the template and write the result to the <see cref="output"/>
    /// </summary>
    /// <param name="output">The text writer to write the result to</param>
    /// <param name="context">The current context of the template service</param>
    /// <param name="arguments">The arguments passed from the template</param>
    void ResolveTemplate(EncodedTextWriter output, object context, object[] arguments);
}