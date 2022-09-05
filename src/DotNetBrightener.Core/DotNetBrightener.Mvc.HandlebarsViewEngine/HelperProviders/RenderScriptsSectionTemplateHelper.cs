using DotNetBrightener.Mvc.HandlebarsViewEngine.TemplateCaching;
using HandlebarsDotNet;
using System;

namespace WebEdFramework.Modular.Mvc.ViewEngines.HelperProviders;

public class RenderScriptsSectionTemplateHelper : ITemplateHelperProvider
{
    public string TemplateSyntaxPrefix => "ScriptsSection";
        
    public string UsageHint => "{{ScriptsSection}}";

    public string Description => "The command to render the scripts included in the requested page";

    public bool IsBlockTemplate => false;

    public void ResolveTemplate(EncodedTextWriter output, object context, object[] arguments)
    {
        if (context is PageRenderContext renderContext)
        {
            var scriptUrlsList = renderContext.GetScriptUrls();
            {
                foreach (var script in scriptUrlsList)
                {
                    var scriptPath = ResolveScriptPath(script, context);
                    output.WriteSafeString($"<script src=\"{scriptPath}\" type=\"text/javascript\"></script>");
                    output.WriteSafeString($"{Environment.NewLine}");
                }
            }

            var scriptContents = renderContext.GetScriptContents();
            {
                foreach (var script in scriptContents)
                {
                    output.WriteSafeString(script);
                    output.WriteSafeString($"{Environment.NewLine}");
                }
            }
        }
    }
        
    private string ResolveScriptPath(string inputStylePath, object context)
    {
        return Handlebars.Compile(inputStylePath)(context);
    }
}