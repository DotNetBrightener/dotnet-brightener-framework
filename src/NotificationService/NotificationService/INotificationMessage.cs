using NotificationService.Entity;

namespace NotificationService;

public interface INotificationMessage
{
    long Id { get; set; }

    /// <summary>
    ///     Prepare a message with the specified type
    /// </summary>
    /// <returns>
    ///     A <see cref="NotificationMessageQueue"/> object that has <see cref="NotificationMessageQueue.NotificationTypeId"/> set to the value of derived type
    /// </returns>
    NotificationMessageQueue PrepareMessage();

    NotificationMessageQueue Clone();
}