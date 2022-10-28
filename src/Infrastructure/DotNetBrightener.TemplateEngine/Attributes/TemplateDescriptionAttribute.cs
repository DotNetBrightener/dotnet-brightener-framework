using System;

namespace DotNetBrightener.TemplateEngine.Attributes
{
    public class TemplateDescriptionAttribute: Attribute
    {
        public string TemplateDescription { get; set; }

        public string TemplateDescriptionKey { get; set; }
    }
}