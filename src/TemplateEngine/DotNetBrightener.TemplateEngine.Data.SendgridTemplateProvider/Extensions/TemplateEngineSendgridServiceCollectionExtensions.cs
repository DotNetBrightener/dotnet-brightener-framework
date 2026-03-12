using DotNetBrightener.TemplateEngine.Data.SendgridTemplateProvider;
using DotNetBrightener.TemplateEngine.Data.SendgridTemplateProvider.Services;
using DotNetBrightener.TemplateEngine.Data.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;

// ReSharper disable CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
///     Extension methods for registering Sendgrid template provider services.
/// </summary>
public static class TemplateEngineSendgridServiceCollectionExtensions
{
    /// <param name="serviceCollection">The service collection.</param>
    extension(IServiceCollection serviceCollection)
    {
        /// <summary>
        ///     Registers the Sendgrid template provider services using configuration from appsettings.json.
        /// </summary>
        /// <param name="configuration">The configuration containing the SendgridTemplateProvider section.</param>
        /// <returns>The service collection for chaining.</returns>
        public IServiceCollection AddTemplateEngineSendgridStorage(IConfiguration configuration)
        {
            var section = configuration.GetSection(SendgridTemplateProviderSettings.ConfigurationSectionName);
            serviceCollection.Configure<SendgridTemplateProviderSettings>(section);

            return serviceCollection.AddSendgridTemplateProviderServices();
        }

        /// <summary>
        ///     Registers the Sendgrid template provider services using an action to configure settings.
        /// </summary>
        /// <param name="configureSettings">An action to configure the settings.</param>
        /// <returns>The service collection for chaining.</returns>
        public IServiceCollection AddTemplateEngineSendgridStorage(
            Action<SendgridTemplateProviderSettings> configureSettings)
        {
            serviceCollection.Configure(configureSettings);

            return serviceCollection.AddSendgridTemplateProviderServices();
        }

        private IServiceCollection AddSendgridTemplateProviderServices()
        {
            // Register the template ID cache as singleton (shared across all scopes)
            serviceCollection.TryAddSingleton<ISendgridTemplateIdCache, SendgridTemplateIdCache>();

            // Register the API client as scoped (uses HttpClient internally)
            serviceCollection.TryAddScoped<ISendgridTemplateApiClient, SendgridTemplateApiClient>();

            // Replace the default template services with Sendgrid implementations
            serviceCollection.Replace(
                                      ServiceDescriptor
                                         .Scoped<ITemplateRegistrationService, SendgridTemplateRegistrationService>());

            serviceCollection.Replace(
                                      ServiceDescriptor
                                         .Scoped<ITemplateStorageService, SendgridTemplateStorageService>());

            return serviceCollection;
        }
    }
}

