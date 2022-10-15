using System.Collections.Concurrent;

namespace DotNetBrightener.Mvc.HandlebarsViewEngine.TemplateCaching;

public class TemplateCacheContainer : ITemplateCacheContainer
{
    private readonly ConcurrentDictionary<string, HandlebarsTemplate<object, object>> _cacheCompilerTemplates = new ConcurrentDictionary<string, HandlebarsTemplate<object, object>>();

    public void StoreTemplate(string key, string templateContent)
    {
        _cacheCompilerTemplates.TryRemove(key, out var _);

        _cacheCompilerTemplates.TryAdd(key, Handlebars.Compile(templateContent));
    }

    public bool RetrieveTemplate(string key, out HandlebarsTemplate<object, object> templateCompilerFunc)
    {
        return _cacheCompilerTemplates.TryGetValue(key, out templateCompilerFunc);
    }
}