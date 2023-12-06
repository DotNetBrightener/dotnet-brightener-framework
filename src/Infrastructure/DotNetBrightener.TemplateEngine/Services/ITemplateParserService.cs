using HandlebarsDotNet;
using System.Collections.Concurrent;

namespace DotNetBrightener.TemplateEngine.Services;

public interface ITemplateParserService 
{
    string ParseTemplate<T>(string inputTemplate, T model);
}

public class TemplateParserService : ITemplateParserService
{
    private readonly ConcurrentDictionary<string, HandlebarsTemplate<object, object>> _cacheCompilerTemplates =
        new();

    public string ParseTemplate<T>(string inputTemplate, T model)
    {
        if (!_cacheCompilerTemplates.TryGetValue(inputTemplate, out var compileFunction))
        {
            compileFunction = Handlebars.Compile(inputTemplate);
            _cacheCompilerTemplates.TryAdd(inputTemplate, compileFunction);
        }

        var parsedTemplate = compileFunction(model);

        parsedTemplate = parsedTemplate.Replace("’", "'")
                                       .Replace("&#8217;", "'");

        return parsedTemplate;
    }
}