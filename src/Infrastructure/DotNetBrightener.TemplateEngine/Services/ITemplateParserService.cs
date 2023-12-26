using HandlebarsDotNet;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace DotNetBrightener.TemplateEngine.Services;

public interface ITemplateParserService 
{
    string ParseTemplate<T>(string inputTemplate, T model, bool isHtml = true);
}

public class TemplateParserService : ITemplateParserService
{
    private readonly ConcurrentDictionary<string, HandlebarsTemplate<object, object>> _cacheCompilerTemplates =
        new();


    private readonly Dictionary<string, string> _deHtmlReplacements = new()
    {
        {
            "&#60;", "<"
        },
        {
            "&#62;", ">"
        },
        {
            "&#38;", "&"
        },
        {
            "&#8217;", "'"
        },
        {
            "&amp;", "&"
        },
        {
            "&lt;", "<"
        },
        {
            "&gt;", ">"
        },
        {
            "&quot;", "\""
        },
        {
            "&nbsp;", " "
        },
        {
            "&copy;", "©"
        },
        {
            "&reg;", "®"
        },
        {
            "&euro;", "€"
        },
        {
            "&pound;", "£"
        },
        {
            "&#169;", "©"
        },
        {
            "&#174;", "®"
        },
        {
            "&#8364;", "€"
        },
        {
            "&#163;", "£"
        },
    };

    public string ParseTemplate<T>(string inputTemplate, T model, bool isHtml = true)
    {
        if (!_cacheCompilerTemplates.TryGetValue(inputTemplate, out var compileFunction))
        {
            compileFunction = Handlebars.Compile(inputTemplate);
            _cacheCompilerTemplates.TryAdd(inputTemplate, compileFunction);
        }

        var parsedTemplate = compileFunction(model);

        if (!isHtml)
        {
            parsedTemplate = WebUtility.HtmlDecode(parsedTemplate);
            //parsedTemplate = _deHtmlReplacements.Aggregate(parsedTemplate,
            //                                            (current, replacement) =>
            //                                                current.Replace(replacement.Key, replacement.Value));
        }

        return parsedTemplate.Replace("’", "'");
    }
}