// ReSharper disable once CheckNamespace

namespace Testcontainers.MsSql;

public static class MsSqlContainerExtensions
{
    public static string GetConnectionString(this MsSqlContainer container, string databaseName = "TestDb")
    {
        return container.GetConnectionString()
                        .Replace("Database=master", databaseName);
    }
}

public abstract class MsSqlServerBaseTest
{
    protected readonly MsSqlContainer MsSqlContainer = new MsSqlBuilder()
                                                        .WithPassword("Str0ng3stP@s5w0rd3ver!")
                                                        .Build();

    protected string ConnectionString => MsSqlContainer.GetConnectionString($"Database=DataMigration_UnitTest{DateTime.Now:yyyyMMddHHmm}");

    [SetUp]
    public async Task Setup()
    {
        await MsSqlContainer.StartAsync();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await MsSqlContainer.DisposeAsync();
    }
}