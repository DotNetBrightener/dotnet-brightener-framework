using DotNetBrightener.Mvc.HandlebarsViewEngine.TemplateCaching;
using HandlebarsDotNet;

namespace WebEdFramework.Modular.Mvc.ViewEngines.HelperProviders
{
    public class RenderBodyTemplateHelper : ITemplateHelperProvider
    {
        public string TemplateSyntaxPrefix => "RenderBody";

        public string UsageHint => "{{RenderBody}}";

        public string Description => "The command to render the real content of the requested resource";

        public bool IsBlockTemplate => false;

        public void ResolveTemplate(EncodedTextWriter output, object context, object[] arguments)
        {
            if (context is PageRenderContext renderContext)
            {
                output.WriteSafeString(renderContext.RenderedOutput ?? string.Empty);
            }
        }
    }
}