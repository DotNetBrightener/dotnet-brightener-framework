using Microsoft.Extensions.Logging;
using NotificationService.Entity;
using NotificationService.Services;

namespace NotificationService.Providers;

public interface INotifyServiceProvider
{
    Type NotificationType { get; }

    Task DeliverSingleNotification(INotificationMessage notificationMessage);

    Task DeliverManyNotifications(IEnumerable<INotificationMessage> notificationMessage);
}

public abstract class BaseNotifyServiceProvider<TNotificationType>(
    INotificationMessageQueueDataService notificationMessageQueueDataService,
    IDateTimeProvider                    dateTimeProvider,
    ILogger                              logger)
    : INotifyServiceProvider
    where TNotificationType : INotificationMessage
{
    protected readonly INotificationMessageQueueDataService NotificationMessageQueueDataService =
        notificationMessageQueueDataService;

    protected readonly IDateTimeProvider DateTimeProvider = dateTimeProvider;
    protected readonly ILogger           Logger           = logger;

    Type INotifyServiceProvider.NotificationType => typeof(TNotificationType);

    Task INotifyServiceProvider.DeliverSingleNotification(INotificationMessage notificationMessage)
    {
        if (notificationMessage is TNotificationType notificationMsg)
        {
            if (notificationMsg.Id == 0)
            {
                Logger.LogInformation("Putting notification to queue");
                var queuedMessage = QueueMessage(notificationMsg);

                if (!queuedMessage.NeedSendImmediately)
                {
                    Logger.LogInformation("No need to send immediately, queue for later");

                    return Task.CompletedTask;
                }

                notificationMsg.Id = queuedMessage.Id;
                Logger.LogInformation("Send immediately is set. Proceeding to send.");
            }

            return DeliverSingleNotification(notificationMsg);
        }

        return Task.CompletedTask;
    }

    Task INotifyServiceProvider.DeliverManyNotifications(IEnumerable<INotificationMessage> notificationMessage)
    {
        var notificationMsgsToSend = notificationMessage.OfType<TNotificationType>();

        return DeliverManyNotifications(notificationMsgsToSend);
    }

    protected abstract Task DeliverSingleNotification(TNotificationType notificationMessage);

    protected abstract Task DeliverManyNotifications(IEnumerable<TNotificationType> notificationMessages);

    protected NotificationMessageQueue QueueMessage(TNotificationType notificationMessage)
    {
        var queuedMessage = notificationMessage.Clone();
        queuedMessage.EnqueuedAtUtc = DateTimeProvider.UtcNow;

        PrepareQueueMessage(notificationMessage, queuedMessage);

        NotificationMessageQueueDataService.Insert(queuedMessage);

        return queuedMessage;
    }

    protected virtual void PrepareQueueMessage(TNotificationType        notificationMessage,
                                               NotificationMessageQueue queueingMessage)
    {

    }
}