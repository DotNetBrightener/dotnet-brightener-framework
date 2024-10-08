using DotNet.Testcontainers.Builders;
using Testcontainers.MsSql;
using Xunit;
using Xunit.Abstractions;

namespace DotNetBrightener.TestHelpers;

public abstract class MsSqlServerBaseXUnitTest(ITestOutputHelper testOutputHelper) : IAsyncLifetime
{
    protected MsSqlContainer MsSqlContainer;


    protected string ConnectionString;

    public async Task InitializeAsync()
    {
        var currentTestingType = GetType().Name;

        var containerName = String.Concat("sqlserver-2022-", currentTestingType, $"-{Guid.NewGuid()}");

        testOutputHelper.WriteLine($"Spinning up container with Name: {containerName}");

        MsSqlContainer = new MsSqlBuilder()
                        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
                        .WithPassword("Str0ng3stP@s5w0rd3ver!")
                        .WithName(containerName)
                        .WithWaitStrategy(Wait.ForUnixContainer()
                                              .UntilMessageIsLogged("SQL Server is now ready for client connections"))
                        .Build();

        await MsSqlContainer.StartAsync();

        testOutputHelper.WriteLine($"Container {containerName} started");

        ConnectionString = MsSqlContainer.GetConnectionString("MsSqlServerBaseTest");
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await MsSqlContainer.DisposeAsync();
    }
}