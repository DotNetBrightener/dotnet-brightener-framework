using System.IO;
using DotNetBrightener.Mvc.HandlebarsViewEngine.TemplateCaching;
using HandlebarsDotNet;

namespace WebEdFramework.Modular.Mvc.ViewEngines.HelperProviders
{
    public class RenderStylesSectionTemplateHelper : ITemplateHelperProvider
    {
        public string TemplateSyntaxPrefix => "StylesSection";
        
        public string UsageHint => "{{StylesSection}}";

        public string Description => "The command to render the styles included in the requested page";

        public bool IsBlockTemplate => false;

        public void ResolveTemplate(EncodedTextWriter output, object context, object[] arguments)
        {
            if (context is PageRenderContext renderContext)
            {
                var styles = renderContext.GetStyleUrls();

                foreach (var style in styles)
                {
                    var stylePath = ResolvePath(style, context);
                    output.WriteSafeString($"<link href=\"{stylePath}\" rel=\"stylesheet\" />");
                }
            }
        }

        public void ResolveBlockTemplate(EncodedTextWriter output, BlockHelperOptions options, object context, object[] arguments)
        {
            throw new System.NotImplementedException();
        }

        private string ResolvePath(string inputStylePath, object context)
        {
            return Handlebars.Compile(inputStylePath)(context);
        }
    }
}