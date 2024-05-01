using DotNetBrightener.TemplateEngine.Models;

namespace DotNetBrightener.TemplateEngine.Data.Services;

/// <summary>
///     Represents the store where templates are registered
/// </summary>
public interface ITemplateStore
{
    /// <summary>
    ///     Registers a template with the given type
    /// </summary>
    /// <typeparam name="TTemplate">Type of the template derived from <see cref="ITemplateModel"/></typeparam>
    /// <returns></returns>
    Task RegisterTemplate<TTemplate>() where TTemplate : ITemplateModel;

    /// <summary>
    ///     Registers a template with the given type, providing the template content and title
    /// </summary>
    /// <typeparam name="TTemplate">Type of the template derived from <see cref="ITemplateModel"/></typeparam>
    /// <param name="templateTitle">The value of the template's title field</param>
    /// <param name="templateContent">The value of the template's content field</param>
    /// <returns></returns>
    Task RegisterTemplate<TTemplate>(string templateTitle, string templateContent) 
        where TTemplate : ITemplateModel;
}