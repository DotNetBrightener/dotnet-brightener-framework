using System;
using DotNetBrightener.TemplateEngine.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetBrightener.TemplateEngine.DependencyInjection;

public static class ApplicationBuilderExtensions
{
    /// <summary>
    ///     Registers template helpers and templates into the system
    ///     before the application finishes launching
    /// </summary>
    /// <param name="serviceProvider"></param>
    /// <returns></returns>
    public static void RegisterTemplateAndHelpers(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();

        var sp                         = scope.ServiceProvider;
        var templateRegistration       = sp.GetService<ITemplateRegistrationService>();
        var templateHelperRegistration = sp.GetService<ITemplateHelperRegistration>();

        templateHelperRegistration?.RegisterHelpers();
        templateRegistration?.RegisterTemplates();
    }
}