namespace DotNetBrightener.Mvc.HandlebarsViewEngine.TemplateCaching;

public interface ITemplateCacheContainer
{
    void StoreTemplate(string key, string templateContent);

    bool RetrieveTemplate(string key, out HandlebarsTemplate<object, object> templateCompilerFunc);
}