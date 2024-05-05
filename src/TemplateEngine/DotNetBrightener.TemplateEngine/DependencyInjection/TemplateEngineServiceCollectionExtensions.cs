using DotNetBrightener.TemplateEngine.Helpers;
using DotNetBrightener.TemplateEngine.Services;
// ReSharper disable CheckNamespace

namespace Microsoft.Extensions.DependencyInjection;

public static class TemplateEngineServiceCollectionExtensions
{
    public static IServiceCollection AddTemplateEngine(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddMemoryCache();

        serviceCollection.AddScoped<ITemplateHelperRegistration, TemplateHelperRegistration>();
        serviceCollection.AddScoped<ITemplateParserService, TemplateParserService>();

        serviceCollection.AddTemplateHelperProvider<DateTimeTemplateHelper>();
        serviceCollection.AddTemplateHelperProvider<FormatCurrencyTemplateHelper>();
        serviceCollection.AddTemplateHelperProvider<SumTemplateHelper>();

        serviceCollection.AddHostedService<TemplateHelperRegistrationStartupService>();

        return serviceCollection;
    }

    public static IServiceCollection
        AddTemplateHelperProvider<TTemplateHelperProvider>(this IServiceCollection serviceCollection)
        where TTemplateHelperProvider : class, ITemplateHelperProvider
    {
        serviceCollection.AddScoped<ITemplateHelperProvider, TTemplateHelperProvider>();

        return serviceCollection;
    }
}