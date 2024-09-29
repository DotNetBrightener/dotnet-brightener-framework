using Testcontainers.MsSql;

namespace DotNetBrightener.TestHelpers;

public abstract class MsSqlServerBaseXUnitTest : IAsyncDisposable
{
    protected readonly MsSqlContainer MsSqlContainer = new MsSqlBuilder()
                                                      .WithPassword("Str0ng3stP@s5w0rd3ver!")
                                                      .Build();

    protected string ConnectionString => MsSqlContainer.GetConnectionString($"MsSqlServerBaseTest_{DateTime.Now:yyyyMMddHHmm}");

    protected MsSqlServerBaseXUnitTest()
    {
        MsSqlContainer.StartAsync().Wait();
    }

    public async ValueTask DisposeAsync()
    {
        await MsSqlContainer.DisposeAsync();
    }
}