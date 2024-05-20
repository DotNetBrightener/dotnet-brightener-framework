namespace DotNetBrightener.SecuredApi.Tests;

internal class UserRecord
{
    public int    Id   { get; set; }
    public string Name { get; set; }
}

internal class SyncUserService : BaseApiHandler<UserRecord>
{
    protected override async Task<UserRecord> ProcessRequest(UserRecord message)
    {
        return new UserRecord
        {
            Id   = 5100,
            Name = "test user"
        };
    }
}