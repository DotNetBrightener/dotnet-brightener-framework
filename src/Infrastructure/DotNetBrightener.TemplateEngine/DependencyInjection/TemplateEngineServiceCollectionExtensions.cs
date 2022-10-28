using DotNetBrightener.TemplateEngine.Helpers;
using DotNetBrightener.TemplateEngine.Services;
// ReSharper disable CheckNamespace

namespace Microsoft.Extensions.DependencyInjection;

public static class TemplateEngineServiceCollectionExtensions
{
    public static void AddTemplateEngine(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<ITemplateContainer, TemplateContainer>();
        serviceCollection.AddScoped<ITemplateHelperRegistration, TemplateHelperRegistration>();
        serviceCollection.AddScoped<ITemplateRegistrationService, TemplateRegistrationService>();
        serviceCollection.AddScoped<ITemplateParserService, TemplateParserService>();
        serviceCollection.AddScoped<ITemplateRecordDataService, TemplateRecordDataService>();
        serviceCollection.AddScoped<ITemplateService, TemplateService>();

        serviceCollection.AddTemplateHelperProvider<DateTimeTemplateHelper>();
        serviceCollection.AddTemplateHelperProvider<FormatCurrencyTemplateHelper>();
        serviceCollection.AddTemplateHelperProvider<SumTemplateHelper>();
    }

    public static IServiceCollection
        AddTemplateProvider<TTemplateProvider>(this IServiceCollection serviceCollection)
        where TTemplateProvider : class, ITemplateProvider
    {
        serviceCollection.AddScoped<ITemplateProvider, TTemplateProvider>();

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