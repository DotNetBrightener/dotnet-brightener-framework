using DotNetBrightener.TemplateEngine.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetBrightener.TemplateEngine.DependencyInjection;

public static class ApplicationBuilderExtensions
{
    /// <summary>
    ///     Registers template helpers and templates into the system
    ///     before the application finishes launching
    /// </summary>
    /// <param name="app"></param>
    /// <returns></returns>
    public static void RegisterTemplateAndHelpers(this ApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();

        var sp                         = scope.ServiceProvider;
        var templateRegistration       = sp.GetService<ITemplateRegistrationService>();
        var templateHelperRegistration = sp.GetService<ITemplateHelperRegistration>();

        templateHelperRegistration?.RegisterHelpers();
        templateRegistration?.RegisterTemplates();
    }
}
