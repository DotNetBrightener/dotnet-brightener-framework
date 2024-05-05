namespace DotNetBrightener.TemplateEngine.Data.Models;

public class TemplateListItemModel
{
    public string TemplateType { get; set; }

    public string TemplateName { get; set; }

    public string TemplateDescription { get; set; }

    public string TemplateDescriptionKey { get; set; }

    public List<string> Fields { get; internal set; }
}