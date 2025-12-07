using Microsoft.Extensions.Configuration;
using NotificationService.Provider.Sendgrid;

// ReSharper disable CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensionsForNotificationService
{
    extension(IServiceCollection serviceCollection)
    {
        /// <summary>
        ///     Configures the notification service to use SendGrid Web API for email delivery.
        /// </summary>
        /// <param name="configuration">The application configuration containing SendGrid settings</param>
        /// <param name="configurationSectionName">
        ///     The name of the configuration section containing SendGrid settings.
        ///     Defaults to "SendgridApiSettings".
        /// </param>
        public void UseSendgridEmailProvider(IConfiguration configuration,
                                             string         configurationSectionName = nameof(SendgridApiSettings))
        {
            serviceCollection.Configure<SendgridApiSettings>(
                configuration.GetSection(configurationSectionName));

            serviceCollection.ReplaceEmailServiceProvider<SendgridEmailNotifyServiceProvider>();
        }
    }
}