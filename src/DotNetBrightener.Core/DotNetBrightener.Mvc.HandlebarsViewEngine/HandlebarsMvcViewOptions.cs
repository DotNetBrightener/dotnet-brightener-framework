using System.Collections.Generic;

namespace DotNetBrightener.Mvc.HandlebarsViewEngine
{
    public class HandlebarsMvcViewOptions
    {
        public IList<string> ViewLocationFormats { get; } = new List<string>();
    }
}