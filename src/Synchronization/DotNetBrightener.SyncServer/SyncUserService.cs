using DotNetBrightener.SecuredApi;

namespace DotNetBrightener.SyncServer;

public class SyncUserService : BaseApiHandler<UserRecord>
{
    private static          long   _syncCount;
    private static readonly Lock SyncLock = new();

    protected override async Task<UserRecord> ProcessRequest(UserRecord message)
    {
        lock (SyncLock)
        {
            message.Id = _syncCount++;
        }

        return message;
    }
}