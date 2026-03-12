using DotNetBrightener.Core.BackgroundTasks;
using NotificationService.Providers;
using NotificationService.Services;

namespace NotificationService.BackgroundTasks;

public class BackgroundNotificationDeliveryService : IBackgroundTask
{
    private readonly INotificationMessageQueueDataService _notificationMsgQueueDataService;
    private readonly IEnumerable<INotifyServiceProvider>  _notifyServiceProviders;
    private readonly IEnumerable<Type>                    _supportedNotificationMessageTypes;

    public BackgroundNotificationDeliveryService(IEnumerable<INotifyServiceProvider> notifyServiceProviders,
                                                 INotificationMessageQueueDataService
                                                     notificationMsgQueueDataService)
    {
        _notifyServiceProviders            = notifyServiceProviders;
        _notificationMsgQueueDataService   = notificationMsgQueueDataService;
        _supportedNotificationMessageTypes = _notifyServiceProviders.Select(p => p.NotificationType);
    }

    public Task Execute()
    {
        var now = DateTimeOffset.UtcNow;

        // ignore the messages that are being sent within last 3 minutes (prevent timeout)
        var excludeLastAttempt = DateTimeOffset.UtcNow.AddMinutes(-3);

        // retrieve the messages to be sent
        var messagesToSend = _notificationMsgQueueDataService.Fetch(q => q.CancelledAtUtc != null &&
                                                                         q.SentAtUtc == null &&
                                                                         (
                                                                             q.NeedSendImmediately ||
                                                                             q.PlanToSendAtUtc == null ||
                                                                             q.PlanToSendAtUtc <= now
                                                                         ) &&
                                                                         (
                                                                             q.LastAttemptUtc == null ||
                                                                             q.LastAttemptUtc < excludeLastAttempt
                                                                         )
                                                                   )
                                                             .ToList();

        if (messagesToSend.Count == 0)
            return Task.CompletedTask;

        List<Task> operations = [];

        foreach (var msgType in _supportedNotificationMessageTypes)
        {
            var notifyServiceProvider = _notifyServiceProviders.FirstOrDefault(p => p.NotificationType == msgType);

            if (notifyServiceProvider == null)
                continue;

            var messagesOfType = messagesToSend.Select(q => q.As(msgType))
                                               .Where(o => o != null)
                                               .OfType<INotificationMessage>()
                                               .ToArray();

            if (messagesOfType.Length == 0)
                continue;

            operations.Add(notifyServiceProvider.DeliverManyNotifications(messagesOfType));
        }

        return Task.WhenAll(operations);
    }
}