using DotNetBrightener.DataAccess.Models;

namespace WebAppCommonShared.Demo.Entities;

public enum SubscriptionStatus
{
    Activated = 1,

    PaymentPending = 100,

    PaymentOverdue = 401,
    
    Cancelled = 500,

    Invalid = 501
}

public class Subscription : BaseEntity
{
    public string Name { get; set; }

    public SubscriptionStatus Status { get; set; }

    public long? UserId { get; set; }
}