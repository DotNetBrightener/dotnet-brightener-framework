using DotNetBrightener.TemplateEngine.Data;
using DotNetBrightener.TemplateEngine.Data.Services;

// ReSharper disable CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class TemplateEngineServiceCollectionExtensions
{
    /// <param name="serviceCollection">
    ///     The <see cref="IServiceCollection"/>
    /// </param>
    extension(IServiceCollection serviceCollection)
    {
        /// <summary>
        ///     Registers the data access services for the template engine to the service collection
        /// </summary>
        public IServiceCollection AddTemplateEngineStorage()
        {
            serviceCollection.AddSingleton<ITemplateContainer, TemplateContainer>();
            serviceCollection.AddScoped<ITemplateRegistrationService, TemplateRegistrationService>();
            serviceCollection.AddScoped<ITemplateStorageService, PhysicalTemplateStorageService>();
            serviceCollection.AddScoped<ITemplateService, DefaultTemplateService>();

            serviceCollection.AddHostedService<TemplateRegistrationStartupTask>();

            return serviceCollection;
        }

        /// <summary>
        ///     Registers the template provider to the service collection
        /// </summary>
        /// <typeparam name="TTemplateProvider">
        ///     The type of the template provider, must be derived from <see cref="ITemplateProvider"/>
        /// </typeparam>
        /// <returns></returns>
        public IServiceCollection AddTemplateProvider<TTemplateProvider>()
            where TTemplateProvider : class, ITemplateProvider
        {
            serviceCollection.AddScoped<ITemplateProvider, TTemplateProvider>();

            return serviceCollection;
        }
    }
}