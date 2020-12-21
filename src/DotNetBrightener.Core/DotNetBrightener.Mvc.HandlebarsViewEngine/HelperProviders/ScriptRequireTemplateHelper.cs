using System.IO;
using DotNetBrightener.Mvc.HandlebarsViewEngine.TemplateCaching;
using HandlebarsDotNet;

namespace WebEdFramework.Modular.Mvc.ViewEngines.HelperProviders
{
    public class ScriptRequireTemplateHelper: ITemplateHelperProvider
    {
        public string TemplateSyntaxPrefix => "ScriptRequire";

        public string UsageHint => "{{ScriptRequire [script_url]}}";

        public string Description => "Command to include a script from given URL to the page and put it before other dependent scripts";

        public bool IsBlockTemplate => false;

        public void ResolveTemplate(EncodedTextWriter output, object context, object[] arguments)
        {
            if (arguments.Length == 0)
                return;

            if (context is PageRenderContext renderContext &&
                arguments[0] is string scriptUrl)
            {
                renderContext.RequireScript(ResolvePath(scriptUrl, context));
            }
        }

        public void ResolveBlockTemplate(EncodedTextWriter output, BlockHelperOptions options, object context, object[] arguments)
        {
            throw new System.NotImplementedException();
        }

        private string ResolvePath(string inputPath, object context)
        {
            return Handlebars.Compile(inputPath)(context);
        }
    }
}