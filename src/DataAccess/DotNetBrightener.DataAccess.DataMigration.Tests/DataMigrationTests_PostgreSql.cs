using DotNetBrightener.DataAccess.DataMigration.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shouldly;
using Testcontainers.PostgreSql;
using Xunit;

namespace DotNetBrightener.DataAccess.DataMigration.Tests;

public class DataMigrationTests_PostgreSql : IAsyncDisposable
{
    private readonly PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder()
                                                               .WithImage("postgres:17")
                                                               .WithDatabase($"DataMigration_UnitTest{DateTime.Now:yyyyMMddHHmm}")
                                                               .WithUsername("test")
                                                               .WithPassword("password")
                                                               .Build();


    public DataMigrationTests_PostgreSql()
    {
        // Start the PostgreSQL container synchronously in constructor
        _postgreSqlContainer.StartAsync().Wait();
    }

    public async ValueTask DisposeAsync()
    {
        // Clean up after each test and dispose container
        TearDownHost();
        await _postgreSqlContainer.DisposeAsync();
    }

    [Fact]
    public async Task AddDataMigrator_ShouldThrowBecauseOfNotInitializeDataMigrationFirst()
    {
        var exception = Should.Throw<InvalidOperationException>(() =>
        {
            var builder = new HostBuilder()
               .ConfigureServices((_, services) =>
                {
                    services.AddDataMigrator<GoodMigration>();
                });

            _ = builder.Build();
        });

        exception.Message.ShouldBe("The data migrations must be enabled first using EnableDataMigrations method");
    }

    [Fact]
    public async Task AddDataMigrator_ShouldThrowBecauseOfNoAttribute()
    {
        var exception =
            Should.Throw<InvalidOperationException>(() => ConfigureService<ShouldNotBeRegisteredMigration>());
        exception.Message
                 .ShouldBe($"The data migration {typeof(ShouldNotBeRegisteredMigration).FullName} must have [DataMigration] attribute defined with the migration id");
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
           .ConfigureServices((_, services) =>
            {
                services.EnableDataMigrations()
                        .UseNpgsql(_postgreSqlContainer.GetConnectionString());

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
           .ConfigureServices((_, services) =>
            {
                services.EnableDataMigrations()
                        .UseNpgsql(_postgreSqlContainer.GetConnectionString());

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
           .ConfigureServices((_, services) =>
            {
                services.EnableDataMigrations();

                services.AddDataMigrator<TMigration>();
            });

        var host = builder.Build();

        return host;
    }

    private void TearDownHost()
    {
        var builder = new HostBuilder()
           .ConfigureServices((_, serviceCollection) =>
            {
                serviceCollection.AddDbContext<DataMigrationDbContext>(options =>
                {
                    options.UseNpgsql(_postgreSqlContainer.GetConnectionString());
                });
            });

        var host = builder.Build();

        using var serviceScope    = host.Services.CreateScope();
        var       serviceProvider = serviceScope.ServiceProvider;

        using var dbContext = serviceProvider.GetRequiredService<DataMigrationDbContext>();
        dbContext.Database.EnsureDeleted();
    }
}