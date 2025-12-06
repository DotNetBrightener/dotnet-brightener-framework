using NotificationService.Entity;

namespace NotificationService.Types;

public class EmailNotificationMessage : NotificationMessageQueue, INotificationMessage
{
    public EmailNotificationMessage()
    {
        NotificationTypeId = nameof(EmailNotificationMessage);
    }

    /// <summary>
    ///     Prepare an <see cref="EmailNotificationMessage"/>
    /// </summary>
    /// <returns>
    ///     A <see cref="NotificationMessageQueue"/> object that has <see cref="NotificationMessageQueue.NotificationTypeId"/>
    ///     set as <see cref="EmailNotificationMessage"/>
    /// </returns>
    public NotificationMessageQueue PrepareMessage()
    {
        return new NotificationMessageQueue
        {
            NotificationTypeId = nameof(EmailNotificationMessage)
        };
    }
}