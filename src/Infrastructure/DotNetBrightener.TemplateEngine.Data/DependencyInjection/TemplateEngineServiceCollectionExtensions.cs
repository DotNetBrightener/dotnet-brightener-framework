using DotNetBrightener.TemplateEngine.Data.Services;
// ReSharper disable CheckNamespace

namespace Microsoft.Extensions.DependencyInjection;

public static class TemplateEngineServiceCollectionExtensions
{
    /// <summary>
    ///     Registers the data access services for the template engine to the service collection
    /// </summary>
    /// <param name="serviceCollection">
    ///     The <see cref="IServiceCollection"/>
    /// </param>
    public static void AddTemplateEngineData(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<ITemplateContainer, TemplateContainer>();
        serviceCollection.AddScoped<ITemplateRegistrationService, TemplateRegistrationService>();
        serviceCollection.AddScoped<ITemplateRecordDataService, TemplateRecordDataService>();
        serviceCollection.AddScoped<ITemplateService, TemplateService>();

    }

    /// <summary>
    ///     Registers the template provider to the service collection
    /// </summary>
    /// <typeparam name="TTemplateProvider">
    ///     The type of the template provider, must be derived from <see cref="ITemplateProvider"/>
    /// </typeparam>
    /// <param name="serviceCollection">
    ///     The <see cref="IServiceCollection"/>
    /// </param>
    /// <returns></returns>
    public static IServiceCollection
        AddTemplateProvider<TTemplateProvider>(this IServiceCollection serviceCollection)
        where TTemplateProvider : class, ITemplateProvider
    {
        serviceCollection.AddScoped<ITemplateProvider, TTemplateProvider>();

        return serviceCollection;
    }
}