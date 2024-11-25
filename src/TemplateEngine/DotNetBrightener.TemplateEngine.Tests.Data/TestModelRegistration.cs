using DotNetBrightener.TemplateEngine.Data.Services;

namespace DotNetBrightener.TemplateEngine.Tests.Data;

internal class TestModelRegistration : ITemplateProvider
{
    public async Task RegisterTemplates(ITemplateStore templateStore)
    {
        await templateStore.RegisterTemplate<TemplateTestModel>("Hello {{Name}}",
                                                                "Hey {{Name}},<br />This is {{Description}}.");
    }
}