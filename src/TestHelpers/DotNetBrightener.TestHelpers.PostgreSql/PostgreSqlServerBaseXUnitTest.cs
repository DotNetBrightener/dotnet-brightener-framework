using Testcontainers.MsSql;
using Testcontainers.PostgreSql;
using Xunit;
using Xunit.Abstractions;

namespace DotNetBrightener.TestHelpers.PostgreSql;

public abstract class PostgreSqlServerBaseXUnitTest(ITestOutputHelper testOutputHelper) : IAsyncLifetime
{
    protected PostgreSqlContainer PostgreSqlContainer;


    protected string ConnectionString;

    public async Task InitializeAsync()
    {
        var currentTestingType = GetType().Name;

        var containerName = String.Concat(currentTestingType, $"-{Guid.NewGuid()}");

        testOutputHelper.WriteLine($"Spinning up container with Name: {containerName}");
        
        PostgreSqlContainer = PostgreSqlContainerGenerator.CreateContainer(containerName);

        await PostgreSqlContainer.StartAsync();

        testOutputHelper.WriteLine($"Container {containerName} started");

        ConnectionString = PostgreSqlContainer.GetConnectionString();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await PostgreSqlContainer.DisposeAsync();
    }
}