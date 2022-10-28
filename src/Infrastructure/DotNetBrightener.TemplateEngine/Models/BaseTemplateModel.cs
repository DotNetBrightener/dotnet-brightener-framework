using System;
using DotNetBrightener.TemplateEngine.Services;

namespace DotNetBrightener.TemplateEngine.Models
{
	public abstract class BaseTemplateModel : ITemplateModel
    {
        public string SiteUrl { get; set; }

        public string Today => DateTime.Today.ToString("MM/dd/yyyy");

        public string CurrentDateTime => DateTimeOffset.Now.ToString("MM/dd/yyyy hh:mm:ss tt, ddd");
    }
}
