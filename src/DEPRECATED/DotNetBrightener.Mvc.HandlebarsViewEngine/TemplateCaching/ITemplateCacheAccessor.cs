using HandlebarsDotNet;

namespace DotNetBrightener.Mvc.HandlebarsViewEngine.TemplateCaching;

/// <summary>
///     Provides methods to access to the template cache
/// </summary>
public interface ITemplateCacheAccessor
{
    /// <summary>
    ///     Store the template to cache. The template will be preprocessed before storing to cache
    /// </summary>
    /// <param name="key">Key of the template</param>
    /// <param name="templateContent">The content of the template, which will be preprocessed before adding to cache</param>
    void StoreTemplate(string key, string templateContent);

    /// <summary>
    ///     Retrieves the template from cache
    /// </summary>
    /// <param name="key">The key of the template</param>
    /// <param name="templateCompilerFunc">The compiler function which will generate the output</param>
    /// <returns></returns>
    bool RetrieveTemplate(string key, out HandlebarsTemplate<object, object> templateCompilerFunc);
}