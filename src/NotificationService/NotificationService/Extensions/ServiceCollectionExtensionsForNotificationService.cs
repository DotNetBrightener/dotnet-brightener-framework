using Microsoft.Extensions.Configuration;
using NotificationService;
using NotificationService.BackgroundTasks;
using NotificationService.Extensions;
using NotificationService.Options;
using NotificationService.Providers;
using NotificationService.Services;

// ReSharper disable CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensionsForNotificationService
{
    extension(IServiceCollection serviceCollection)
    {
        public NotificationServiceBuilder EnableNotificationService(IConfiguration configuration)
        {
            serviceCollection.Configure<EmailSmtpSetting>(configuration.GetSection(nameof(EmailSmtpSetting)));

            serviceCollection
               .Configure<EmailNotificationSettings>(configuration.GetSection(nameof(EmailNotificationSettings)));

            serviceCollection.AddScoped<INotifyService, NotifyService>();
            serviceCollection.AddScoped<INotificationMessageQueueDataService, NotificationMessageQueueDataService>();
            serviceCollection.AddScoped<INotifyServiceProvider, EmailNotifyServiceProvider>();

            serviceCollection.AddBackgroundTask<BackgroundNotificationDeliveryService>();

            var notificationServiceBuilder = new NotificationServiceBuilder
            {
                Services = serviceCollection
            };

            serviceCollection.AddSingleton(notificationServiceBuilder);

            return notificationServiceBuilder;
        }

        public void ReplaceEmailServiceProvider<TEmailNotifyServiceProvider>()
            where TEmailNotifyServiceProvider : class, INotifyServiceProvider
        {
            var serviceProvider =
                serviceCollection.FirstOrDefault(d => d.ServiceType == typeof(INotifyServiceProvider) &&
                                                      d.ImplementationType ==
                                                      typeof(EmailNotifyServiceProvider));
            if (serviceProvider is not null)
                serviceCollection.Remove(serviceProvider);

            serviceCollection.AddScoped<INotifyServiceProvider, TEmailNotifyServiceProvider>();
        }
    }
}