using System.IO;

namespace DotNetBrightener.TemplateEngine.Services
{
    public interface ITemplateHelperProvider 
    {
        string HelperName { get; }

        string UsageHint { get; }

        void ResolveTemplate(TextWriter output, object context, object[] arguments);
    }
}