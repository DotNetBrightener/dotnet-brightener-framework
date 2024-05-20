namespace DotNetBrightener.SyncServer;

/// <summary>
///     Declares an entity that can be synchronized between services or between server and client
/// </summary>
public interface ISynchronizableEntity
{
    /// <summary>
    ///     The identifier of the entity that should be unique synchronized between services
    /// </summary>
    Guid SyncId { get; set; }

    /// <summary>
    ///     Indicates when the last time the synchronization was done
    /// </summary>
    DateTimeOffset? LastSyncedUtc { get; set; }
}

public interface ISynchronizableEntity<TKeyType> : ISynchronizableEntity where TKeyType : struct
{
    TKeyType Id { get; set; }
}