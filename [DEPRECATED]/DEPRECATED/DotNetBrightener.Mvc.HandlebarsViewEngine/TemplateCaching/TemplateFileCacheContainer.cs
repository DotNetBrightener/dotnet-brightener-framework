namespace DotNetBrightener.Mvc.HandlebarsViewEngine.TemplateCaching;

public class TemplateFileCacheContainer : ITemplateFileCacheContainer
{
    private readonly ITemplateCacheAccessor _templateCacheAccessor;

    public TemplateFileCacheContainer(ITemplateCacheAccessor templateCacheAccessor)
    {
        _templateCacheAccessor = templateCacheAccessor;
    }

    private const string CacheKeyPrefix = "TEMPLATE-FILE::";

    public void StoreTemplate(string filePath, string templateContent)
    {
        _templateCacheAccessor.StoreTemplate(GetCacheKey(filePath), templateContent);
    }

    public bool TryGetTemplate(string filePath, out HandlebarsTemplate<object, object> templateCompilerFunc)
    {
        return _templateCacheAccessor.RetrieveTemplate(GetCacheKey(filePath), out templateCompilerFunc);
    }

    private string GetCacheKey(string filePath)
    {
        return $"{CacheKeyPrefix}{filePath}".ToUpperInvariant();
    }
}