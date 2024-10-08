using NUnit.Framework;
using Testcontainers.MsSql;

namespace DotNetBrightener.TestHelpers;

public abstract class MsSqlServerBaseNUnitTest
{
    protected readonly MsSqlContainer MsSqlContainer = new MsSqlBuilder()
                                                      .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
                                                      .WithPassword("Str0ng3stP@s5w0rd3ver!")
                                                      .Build();

    protected string ConnectionString =>
        MsSqlContainer.GetConnectionString($"MsSqlServerBaseTest_{DateTime.Now:yyyyMMddHHmm}");

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