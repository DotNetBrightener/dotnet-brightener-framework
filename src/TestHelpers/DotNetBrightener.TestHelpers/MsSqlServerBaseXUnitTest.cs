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
        try
        {
            var currentTestingType = GetType().Name;

            var containerName = string.Concat("sqlserver-2022-", currentTestingType, $"-{Guid.NewGuid()}");

            testOutputHelper.WriteLine($"Spinning up container with Name: {containerName}");

            MsSqlContainer = MsSqlContainerGenerator.CreateContainer(containerName);

            await MsSqlContainer.StartAsync();

            testOutputHelper.WriteLine($"Container {containerName} started");

            ConnectionString = MsSqlContainer.GetConnectionStringForDb("MsSqlServerBaseTest");
        }
        catch (Exception)
        {
            if (MsSqlContainer is not null)
                await MsSqlContainer.DisposeAsync();

            throw;
        }
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await MsSqlContainer.DisposeAsync();
    }
}