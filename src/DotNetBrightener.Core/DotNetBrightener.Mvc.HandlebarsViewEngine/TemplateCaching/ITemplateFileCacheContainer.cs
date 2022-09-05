namespace DotNetBrightener.Mvc.HandlebarsViewEngine.TemplateCaching;

public interface ITemplateFileCacheContainer
{
    void StoreTemplate(string filePath, string templateContent);

    bool TryGetTemplate(string filePath, out HandlebarsTemplate<object, object> templateCompilerFunc);
}