using HandlebarsDotNet;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Net;

namespace DotNetBrightener.TemplateEngine.Services;

public interface ITemplateParserService
{
    string ParseTemplate<T>(string inputTemplate, T model, bool isHtml = true);

    Task<string> ParseTemplateAsync<T>(string inputTemplate, T model, bool isHtml = true);
}

public class TemplateParserService(IMemoryCache memoryCache, 
                                   ILogger<TemplateParserService> logger) : ITemplateParserService
{
    public string ParseTemplate<T>(string inputTemplate, T model, bool isHtml = true)
    {
        var result = ParseTemplateAsync(inputTemplate, model, isHtml).Result;

        return result;
    }

    public async Task<string> ParseTemplateAsync<T>(string inputTemplate, T model, bool isHtml = true)
    {
        if (string.IsNullOrEmpty(inputTemplate))
            return string.Empty;

        var compileFunction = await GetOrSet(inputTemplate);

        if (compileFunction is null)
            return string.Empty;

        var parsedTemplate = compileFunction(model);

        if (!isHtml)
        {
            parsedTemplate = WebUtility.HtmlDecode(parsedTemplate);
        }

        return parsedTemplate.Replace("’", "'")
                             .Replace("&#8217;", "'")
                             .Replace("&#65533;", "'");
    }

    private async Task<HandlebarsTemplate<object, object>> GetOrSet(string templateString)
    {
        var result = await memoryCache.GetOrCreateAsync(templateString,
                                                        async entry =>
                                                        {
                                                            logger
                                                               .LogInformation("No result found in cache. Acquiring result...");

                                                            entry.SetOptions(PrepareCacheEntryOptions());

                                                            var compileFunction = Handlebars.Compile(templateString);

                                                            return compileFunction;
                                                        });
        if (result is null)
            memoryCache.Remove(templateString);

        return result;
    }

    private static MemoryCacheEntryOptions PrepareCacheEntryOptions()
    {
        //set expiration time for the passed cache key
        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
        };

        return options;
    }
}