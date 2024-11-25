using DotNetBrightener.TemplateEngine.Models;

namespace DotNetBrightener.TemplateEngine.Tests.Data;

internal class TemplateTestModel : ITemplateModel
{
    public string Name { get; set; }

    public string Description { get; set; }
}