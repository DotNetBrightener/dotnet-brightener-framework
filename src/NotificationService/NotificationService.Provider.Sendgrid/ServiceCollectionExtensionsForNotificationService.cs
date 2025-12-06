
using NotificationService.Extensions;
using NotificationService.Provider.Sendgrid;

// ReSharper disable CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensionsForNotificationService
{
    extension(IServiceCollection serviceCollection)
    {
        public void UseSendgridEmailProvider()
        {
            serviceCollection.ReplaceEmailServiceProvider<SendgridEmailNotifyServiceProvider>();
        }
    }
}