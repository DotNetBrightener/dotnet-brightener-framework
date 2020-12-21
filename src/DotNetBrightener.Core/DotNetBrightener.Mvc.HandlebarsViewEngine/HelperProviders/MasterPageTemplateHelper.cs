using DotNetBrightener.Mvc.HandlebarsViewEngine.TemplateCaching;
using HandlebarsDotNet;

namespace WebEdFramework.Modular.Mvc.ViewEngines.HelperProviders
{
    public class MasterPageTemplateHelper: ITemplateHelperProvider
    {
        public string TemplateSyntaxPrefix => "MasterPage";

        public string UsageHint => "{{MasterPage [master_page_name]}}";

        public string Description => "Command to select and compose the master template";

        public bool IsBlockTemplate => false;

        public void ResolveTemplate(EncodedTextWriter output, object context, object[] arguments)
        {
            if (arguments.Length == 0)
                return;

            if (context is PageRenderContext renderContext)
            {
                if (arguments[0] is string templateName)
                {
                    renderContext.PendingMasterPageTemplatePath = templateName;
                }
            }
        }
    }
}