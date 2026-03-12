using DotNetBrightener.Core.BackgroundTasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NotificationService.Entity;
using NotificationService.Providers;

namespace NotificationService;

public interface INotifyService
{
    Task DeliverNotification<TNotificationMessage>(TNotificationMessage notificationMessage)
        where TNotificationMessage : INotificationMessage;
}

public class NotifyService(
    IScheduler             scheduler,
    IServiceScopeFactory   serviceScopeFactory,
    ILogger<NotifyService> logger)
    : INotifyService
{
    private readonly ILogger _logger = logger;

    public virtual async Task DeliverNotification<TNotificationMessage>(TNotificationMessage notificationMessage)
        where TNotificationMessage : INotificationMessage
    {
        if (notificationMessage is not NotificationMessageQueue queuedMessage ||
            queuedMessage.NeedSendImmediately == false)
        {
            var methodInfo = this.GetMethodWithName(nameof(BackgroundDeliverNotification));

            scheduler.ScheduleTask(methodInfo,
                                   notificationMessage,
                                   notificationMessage.GetType())
                     .Once();
        }
        else
        {
            await BackgroundDeliverNotification(notificationMessage, notificationMessage.GetType());
        }
    }

    private async Task BackgroundDeliverNotification(INotificationMessage notificationMessage, Type notifyServiceType)
    {
        using var scope = serviceScopeFactory.CreateScope();

        try
        {
            var notifyServiceProvider = scope.ServiceProvider
                                             .GetServices<INotifyServiceProvider>()
                                             .FirstOrDefault(p => p.NotificationType == notifyServiceType);

            if (notifyServiceProvider == null)
                return;

            await notifyServiceProvider.DeliverSingleNotification(notificationMessage);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception,
                             "Error while deliver notification of type {notifyServiceType}",
                             notifyServiceType);
        }
    }
}