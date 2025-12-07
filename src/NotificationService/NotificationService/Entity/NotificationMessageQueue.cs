using System.ComponentModel.DataAnnotations;
using DotNetBrightener.DataAccess.Models;

namespace NotificationService.Entity;

public class NotificationMessageQueue : BaseEntity
{
    /// <summary>
    ///     Indicates the type of the notification, eg. SMS / Email / Push Notification
    /// </summary>
    [MaxLength(255)]
    public string NotificationTypeId { get; set; }

    /// <summary>
    ///     Indicates the target of the notification, eg. phone number, or email address
    /// </summary>
    [MaxLength(320)]
    public string DeliveryTarget { get; set; }

    /// <summary>
    ///     The target for carbon-copying, only used for email
    /// </summary>
    [MaxLength(2048)]
    public string CcTargets { get; set; } = string.Empty;

    /// <summary>
    ///     The target for blinded-carbon-copying, only used for email
    /// </summary>
    [MaxLength(2048)]
    public string BccTargets { get; set; } = string.Empty;

    /// <summary>
    ///     Indicates the main content of the message.
    /// 
    ///     It can be the email body if the notification type is Email, or the SMS content of the type is SMS
    /// </summary>
    public string MessageBody { get; set; }

    /// <summary>
    ///     Indicates the title of the message. It will be ignored if the type is SMS
    /// </summary>
    [MaxLength(1024)]
    public string MessageTitle { get; set; }

    /// <summary>
    ///     The identifier of the entity which is used for delivering the message,
    ///     eg the associated phone number or associated email address
    /// </summary>
    public long SenderEntityId { get; set; }

    /// <summary>
    ///     Indicates the message should be sent immediately
    /// </summary>
    public bool NeedSendImmediately { get; set; }

    /// <summary>
    ///     The date and time the message is placed to the queue
    /// </summary>
    public DateTimeOffset? EnqueuedAtUtc { get; set; }

    /// <summary>
    ///     The date and time to deliver the message, if <see cref="NeedSendImmediately"/> is set to <c>false</c>
    /// </summary>
    public DateTimeOffset? PlanToSendAtUtc { get; set; }

    /// <summary>
    ///     The date and time the message has been delivered
    /// </summary>
    public DateTimeOffset? SentAtUtc { get; set; }

    /// <summary>
    ///     The date and time the system tried to deliver the message
    /// </summary>
    public DateTimeOffset? LastAttemptUtc { get; set; }

    /// <summary>
    ///     The exception thrown in last attempt.
    /// </summary>
    public string LastAttemptException { get; set; }

    /// <summary>
    ///     The date and time the sending of message has been cancelled
    /// </summary>
    public DateTimeOffset? CancelledAtUtc { get; set; }

    /// <summary>
    ///     Creates a cloned message
    /// </summary>
    /// <returns>
    ///     The cloned notification message
    /// </returns>
    public NotificationMessageQueue Clone()
    {
        var cloned = new NotificationMessageQueue();

        this.CopyTo(cloned, ignoredProperties: q => q.Id);

        return cloned;
    }

    /// <summary>
    ///     Cast the current message as <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of destination type to cast</typeparam>
    /// <returns>
    ///     The instance of this message cast to <typeparamref name="T"/>.
    ///     If the message is an instance of that type, return it.
    ///     Otherwise, clone the object and cast it to <typeparamref name="T"/> and return it.
    /// </returns>
    public T As<T>() where T : INotificationMessage
    {
        if (this is T tInstance)
            return tInstance;

        if (NotificationTypeId != typeof(T).Name)
            return default;

        tInstance = Activator.CreateInstance<T>();

        this.CopyTo(tInstance);

        return tInstance;
    }

    public object As(Type targetType)
    {
        if (NotificationTypeId != targetType.Name)
            return default;

        var tInstance = Activator.CreateInstance(targetType);

        this.CopyTo(tInstance);

        return tInstance;
    }
}