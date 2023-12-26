using System.Collections.Generic;

namespace DotNetBrightener.TemplateEngine.Models;

public class TemplateModelDto
{
    public string TemplateType { get; set; }

    public string TemplateContent { get; set; }

    public string TemplateTitle { get; set; }

    public List<string> Fields { get; set; }
}