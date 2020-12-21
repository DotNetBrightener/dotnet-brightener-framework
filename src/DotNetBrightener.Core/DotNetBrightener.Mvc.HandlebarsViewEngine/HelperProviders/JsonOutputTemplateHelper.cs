using DotNetBrightener.Mvc.HandlebarsViewEngine.TemplateCaching;
using HandlebarsDotNet;
using Newtonsoft.Json;
using System.Linq;

namespace DotNetBrightener.Mvc.HandlebarsViewEngine.HelperProviders
{
    public class JsonOutputTemplateHelper : ITemplateHelperProvider
    {
        public string TemplateSyntaxPrefix => "JsonOutput";

        public string UsageHint => "{{JsonOutput [your-data]}}";

        public string Description => "The command to render the given data as json format in a script tag";
        public bool IsBlockTemplate => false;

        public void ResolveTemplate(EncodedTextWriter output, object context, object[] arguments)
        {
            if (arguments.Length == 0)
                return;

            output.WriteSafeString(JsonConvert.SerializeObject(arguments.FirstOrDefault()));
        }
    }
}