using DotNetBrightener.DataAccess.DataMigration.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using Testcontainers.PostgreSql;

namespace DotNetBrightener.DataAccess.DataMigration.Tests;

internal class DataMigrationTests_PostgreSql
{
    private readonly PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder()
                                                               .WithDatabase($"DataMigration_UnitTest{DateTime.Now:yyyyMMddHHmm}")
                                                               .WithUsername("test")
                                                               .WithPassword("password")
                                                               .Build();


    [SetUp]
    public async Task Setup()
    {
        await _postgreSqlContainer.StartAsync();
    }

    [TearDown]
    public async Task TearDown()
    {
        TearDownHost();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await _postgreSqlContainer.DisposeAsync();
    }

    [Test]
    public async Task AddDataMigrator_ShouldThrowBecauseOfNotInitializeDataMigrationFirst()
    {
        Assert.Throws(Is.TypeOf<InvalidOperationException>()
                        .And.Message
                        .EqualTo("The data migrations must be enabled first using EnableDataMigrations method"),
                      () =>
                      {
                          var builder = new HostBuilder()
                             .ConfigureServices((_, services) =>
                              {
                                  services.AddDataMigrator<GoodMigration>();
                              });

                          _ = builder.Build();
                      });
    }

    [Test]
    public async Task AddDataMigrator_ShouldThrowBecauseOfNoAttribute()
    {
        Assert.Throws(Is.TypeOf<InvalidOperationException>()
                        .And.Message
                        .EqualTo($"The data migration {typeof(ShouldNotBeRegisteredMigration).FullName} must have [DataMigration] attribute defined with the migration id"),
                      () => ConfigureService<ShouldNotBeRegisteredMigration>());
    }

    [Test]
    public async Task AddDataMigrator_ShouldRegisterWithoutIssue()
    {
        // Arrange
        var host = ConfigureService<GoodMigration>();

        // Act
        var serviceProvider = host.Services;
        var metadata        = serviceProvider.GetRequiredService<DataMigrationMetadata>();

        // Assert
        Assert.That(metadata.Values.Count, Is.EqualTo(1));
        Assert.That(metadata.Values.ElementAt(0), Is.EqualTo(typeof(GoodMigration)));
    }

    [Test]
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
        Assert.That(migrationHistory.Count, Is.EqualTo(2));
        Assert.That(migrationHistory[0].MigrationId, Is.EqualTo("20240502_160412_InitializeMigration"));
        Assert.That(migrationHistory[1].MigrationId, Is.EqualTo("20240502_160413_InitializeMigration3"));

        await host.StopAsync();
    }

    [Test]
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
        Assert.That(migrationHistory.Count, Is.EqualTo(0));

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