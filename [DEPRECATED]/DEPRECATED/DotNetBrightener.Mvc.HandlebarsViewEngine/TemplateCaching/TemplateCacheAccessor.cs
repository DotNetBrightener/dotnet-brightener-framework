namespace DotNetBrightener.Mvc.HandlebarsViewEngine.TemplateCaching;

public class TemplateCacheAccessor : ITemplateCacheAccessor
{
    private readonly ITemplateCacheContainer _templateCacheContainer;

    public TemplateCacheAccessor(ITemplateCacheContainer templateCacheContainer)
    {
        _templateCacheContainer = templateCacheContainer;
    }

    public void StoreTemplate(string key, string templateContent)
    {
        _templateCacheContainer.StoreTemplate(key, templateContent);
    }

    public bool RetrieveTemplate(string key, out HandlebarsTemplate<object, object> templateCompilerFunc)
    {
        return _templateCacheContainer.RetrieveTemplate(key, out templateCompilerFunc);
    }
}