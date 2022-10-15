using System.IO;
using DotNetBrightener.Mvc.HandlebarsViewEngine.TemplateCaching;
using HandlebarsDotNet;

namespace WebEdFramework.Modular.Mvc.ViewEngines.HelperProviders;

public class ScriptIncludeTemplateHelper: ITemplateHelperProvider
{
    public string TemplateSyntaxPrefix => "ScriptInclude";

    public string UsageHint => "{{ScriptInclude [script_url]}}";

    public string Description => "Command to include a script from given URL to the page";

    public bool IsBlockTemplate => false;

    public void ResolveTemplate(EncodedTextWriter output, object context, object[] arguments)
    {
        if (arguments.Length == 0)
            return;

        if (context is PageRenderContext renderContext &&
            arguments[0] is string scriptUrl)
        {
            renderContext.IncludeScript(ResolvePath(scriptUrl, context));
        }
    }

    public void ResolveBlockTemplate(EncodedTextWriter output, BlockHelperOptions options, object context, object[] arguments)
    {
        throw new System.NotImplementedException();
    }

    private string ResolvePath(string inputScriptPath, object context)
    {
        return Handlebars.Compile(inputScriptPath)(context);
    }
}