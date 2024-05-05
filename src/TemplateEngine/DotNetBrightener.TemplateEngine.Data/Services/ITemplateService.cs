using DotNetBrightener.TemplateEngine.Data.Models;
using DotNetBrightener.TemplateEngine.Models;

namespace DotNetBrightener.TemplateEngine.Data.Services;

/// <summary>
///     Represents the service that is responsible for managing, manipulating and parsing the templates
/// </summary>
public interface ITemplateService
{
    /// <summary>
    ///     Retrieves all available templates
    /// </summary>
    /// <returns></returns>
    List<TemplateListItemModel> GetAllAvailableTemplates();

    /// <summary>
    ///    Saves the specified template with the given type
    /// </summary>
    /// <param name="templateType">
    ///     The template type
    /// </param>
    /// <param name="content">
    ///     The model represents the template data, e.g. title, content, etc.
    /// </param>
    void SaveTemplate(string templateType, TemplateModelDto content);

    /// <summary>
    ///     Loads the template for specified type <seealso cref="TTemplate"/>
    /// </summary>
    /// <typeparam name="TTemplate">
    ///     The type of the template to load
    /// </typeparam>
    /// <returns>
    ///     The template model
    /// </returns>
    TemplateModelDto LoadTemplate<TTemplate>() where TTemplate : ITemplateModel;

    /// <summary>
    ///    Loads the template for specified type <see cref="templateModelType"/>
    /// </summary>
    /// <param name="templateModelType">
    ///     The full name of the template type to load 
    /// </param>
    /// <returns>
    ///     The template model
    /// </returns>
    TemplateModelDto LoadTemplate(string templateModelType);

    /// <summary>
    ///     Loads the template from storage and then parse it with the provided model instance
    /// </summary>
    /// <typeparam name="TTemplate">The type of <see cref="instance"/> object</typeparam>
    /// <param name="instance">The model used to parse the template content</param>
    /// <returns></returns>
    TemplateModelDto LoadAndParseTemplate<TTemplate>(TTemplate instance, bool isHtml = true)
        where TTemplate : ITemplateModel;

    /// <summary>
    ///     Loads the template from storage and then parse it with the provided model instance
    /// </summary>
    /// <typeparam name="TTemplate">
    ///     The type of <see cref="instance"/> object
    /// </typeparam>
    /// <param name="instance">
    ///     The model used to parse the template content
    /// </param>
    /// <returns></returns>
    Task<TemplateModelDto> LoadAndParseTemplateAsync<TTemplate>(TTemplate instance, bool isHtml = true)
        where TTemplate : ITemplateModel;

    string ParseTemplate(string template, dynamic instance, bool isHtml = true);

    Task<string> ParseTemplateAsync(string template, dynamic instance, bool isHtml = true);
}