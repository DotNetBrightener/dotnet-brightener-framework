using DotNetBrightener.DataAccess.DataMigration.Extensions;
using DotNetBrightener.TestHelpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace DotNetBrightener.DataAccess.DataMigration.Tests;

public class DataMigrationTests_SqlServer(ITestOutputHelper testOutputHelper): MsSqlServerBaseXUnitTest(testOutputHelper)
{
    [Fact]
    public async Task AddDataMigrator_ShouldThrowBecauseOfNotInitializeDataMigrationFirst()
    {
        var exception = Assert.Throws<InvalidOperationException>(() =>
        {
            var builder = new HostBuilder()
               .ConfigureServices((hostContext, services) =>
                {
                    services.AddDataMigrator<GoodMigration>();
                });

            var host = builder.Build();
        });

        exception.Message.ShouldBe("The data migrations must be enabled first using EnableDataMigrations method");
    }

    [Fact]
    public async Task AddDataMigrator_ShouldThrowBecauseOfNoAttribute()
    {
        var exception = Assert.Throws<InvalidOperationException>(() => ConfigureService<ShouldNotBeRegisteredMigration>());

        exception.Message.ShouldBe($"The data migration {typeof(ShouldNotBeRegisteredMigration).FullName} must have [DataMigration] attribute defined with the migration id");
    }

    [Fact]
    public async Task AddDataMigrator_ShouldRegisterWithoutIssue()
    {
        // Arrange
        var host = ConfigureService<GoodMigration>();

        // Act
        var serviceProvider = host.Services;
        var metadata        = serviceProvider.GetRequiredService<DataMigrationMetadata>();

        // Assert
        metadata.Values.Count.ShouldBe(1);
        metadata.Values.ElementAt(0).ShouldBe(typeof(GoodMigration));
    }

    [Fact]
    public async Task AddDataMigrator_ShouldExecuteAtAppStart()
    {
        // Arrange
        var builder = new HostBuilder()
           .ConfigureServices((hostContext, services) =>
            {
                services.EnableDataMigrations()
                        .UseSqlServer(ConnectionString);

                services.AddDataMigrator<GoodMigration>();
                services.AddDataMigrator<GoodMigration2>();
            });

        var host = builder.Build();

        // Acts
        await host.StartAsync();

        using var serviceScope = host.Services.CreateScope();

        var serviceProvider = serviceScope.ServiceProvider;

        await using var dbContext = serviceProvider.GetRequiredService<DataMigrationDbContext>();

        var migrationHistory = dbContext.Set<DataMigrationHistory>()
                                        .ToList();

        // Assert
        migrationHistory.Count.ShouldBe(2);
        migrationHistory[0].MigrationId.ShouldBe("20240502_160412_InitializeMigration");
        migrationHistory[1].MigrationId.ShouldBe("20240502_160413_InitializeMigration3");

        await host.StopAsync();
    }

    [Fact]
    public async Task AddDataMigrator_ShouldExecuteAtAppStart_WithoutWritingHistoryDueToException()
    {
        // Arrange
        var builder = new HostBuilder()
           .ConfigureServices((hostContext, services) =>
            {
                services.EnableDataMigrations()
                        .UseSqlServer(ConnectionString);

                services.AddDataMigrator<GoodMigration>();
                services.AddDataMigrator<MigrationWithThrowingException>();
            });

        var host = builder.Build();

        // Acts
        await host.StartAsync();

        using var serviceScope = host.Services.CreateScope();

        var serviceProvider = serviceScope.ServiceProvider;

        await using var dbContext = serviceProvider.GetRequiredService<DataMigrationDbContext>();

        var migrationHistory = dbContext.Set<DataMigrationHistory>()
                                        .ToList();

        // Assert
        migrationHistory.Count.ShouldBe(0);

        await host.StopAsync();
    }

    private IHost ConfigureService<TMigration>() where TMigration : IDataMigration
    {
        var builder = new HostBuilder()
           .ConfigureServices((hostContext, services) =>
            {
                services.EnableDataMigrations();

                services.AddDataMigrator<TMigration>();
            });

        var host = builder.Build();

        return host;
    }
}
