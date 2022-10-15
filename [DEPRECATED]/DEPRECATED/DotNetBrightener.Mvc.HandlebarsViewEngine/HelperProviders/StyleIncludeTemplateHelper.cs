using System.IO;
using DotNetBrightener.Mvc.HandlebarsViewEngine.TemplateCaching;
using HandlebarsDotNet;

namespace WebEdFramework.Modular.Mvc.ViewEngines.HelperProviders;

public class StyleIncludeTemplateHelper: ITemplateHelperProvider
{
    public string TemplateSyntaxPrefix => "StyleInclude";

    public string UsageHint => "{{StyleInclude [style_url]}}";

    public string Description => "Command to include a style from given URL to the page";

    public bool IsBlockTemplate => false;

    public void ResolveTemplate(EncodedTextWriter output, object context, object[] arguments)
    {
        if (arguments.Length == 0)
            return;

        if (context is PageRenderContext renderContext &&
            arguments[0] is string scriptUrl)
        {
            renderContext.AddStyleUrl(ResolvePath(scriptUrl, context));
        }
    }

    private string ResolvePath(string inputScriptPath, object context)
    {
        return Handlebars.Compile(inputScriptPath)(context);
    }
}