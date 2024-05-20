using DotNetBrightener.DataAccess.Models;
using DotNetBrightener.SyncServer;

public abstract class SynchronizableEntityWithAuditInfo : BaseEntityWithAuditInfo, ISynchronizableEntity<long>
{
    public DateTimeOffset? LastSyncedUtc
    {
        get => ModifiedDate;
        set => ModifiedDate = value;
    }

    public Guid SyncId { get; set; }
}

public class UserRecord : SynchronizableEntityWithAuditInfo
{
    public string Name { get; set; }

    public string Email { get; set; }
}