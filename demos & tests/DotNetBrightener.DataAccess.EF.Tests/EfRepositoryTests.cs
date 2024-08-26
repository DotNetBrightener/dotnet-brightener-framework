using DotNetBrightener.DataAccess.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel.DataAnnotations;
using DotNetBrightener.DataAccess.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace DotNetBrightener.DataAccess.EF.Tests;

public class TestEntity: GuidBaseEntityWithAuditInfo
{

    [MaxLength(512)]
    public string Value { get; set; }
}

public class TestDbContext : DbContext
{
    public TestDbContext(DbContextOptions<TestDbContext> options)
        : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TestEntity>();
    }
}

internal class UserResolver : ICurrentLoggedInUserResolver
{
    public string CurrentUserName => "Unit Test Runner";
    public string CurrentUserId   => "UnitTestRunner";
}

public class EfRepositoryTests
{
    private ServiceProvider _serviceProvider;

    //private static readonly string DateTimeString = DateTime.Now.ToString("yyyyMMddHHmmss");

    //private static readonly string ConnectionString = $"Data Source=192.168.20.163;Initial Catalog=dnb_testdb_{DateTimeString};User ID=sa;Password=sCpTXbW8jbSbbUpILfZVulTiwqcPyJWt;MultipleActiveResultSets=True;Encrypt=True;TrustServerCertificate=True;";

    private static readonly string ConnectionString = $"Data Source=TestDb.db;";

    [SetUp]
    public void Setup()
    {
        var serviceCollection = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        serviceCollection.AddEFCentralizedDataServices<TestDbContext>(new DatabaseConfiguration
                                                                        {
                                                                            ConnectionString = ConnectionString,
                                                                            DatabaseProvider = DatabaseProvider.Sqlite
                                                                        },
                                                                        configuration,
                                                                        optionBuilder =>
                                                                        {
                                                                            optionBuilder.UseSqlite(ConnectionString);
                                                                        });

        serviceCollection.AddLogging(builder => builder.AddConsole());
        serviceCollection.AddEventPubSubService();
        serviceCollection.AddEventHandlersFromAssemblies();
        serviceCollection.Replace(ServiceDescriptor.Scoped<ICurrentLoggedInUserResolver, UserResolver>());

        _serviceProvider = serviceCollection.BuildServiceProvider();

        var dbContext = _serviceProvider.GetService<TestDbContext>();
        dbContext!.Database.EnsureCreated();
    }

    [TearDown]
    public void TearDown()
    {
        var dbContext = _serviceProvider.GetService<TestDbContext>();
        dbContext!.Database.EnsureDeleted();

        _serviceProvider.Dispose();
    }

    [Test]
    public void TestRepositoryUpdate_ShouldBeAbleToUpdateWithoutRetrievingData()
    {
        var repository = _serviceProvider.GetService<IRepository>();

        Assert.That(repository, Is.Not.Null);

        var testInstance = new TestEntity
        {
            Value = Guid.NewGuid().ToString()
        };

        repository!.Insert(testInstance);
        repository.CommitChanges();

        repository.Update<TestEntity>(_ => _.Id == testInstance.Id,
                                      source => new TestEntity
                                      {
                                          Value = source.Value + "_updated"
                                      });
        var dbContext = _serviceProvider.GetService<TestDbContext>();

        var storedInstance = dbContext!.Set<TestEntity>().FirstOrDefault(_ => _.Id == testInstance.Id);

        Assert.That(storedInstance, Is.Not.Null);

        Assert.That(storedInstance!.Value, Is.EqualTo(testInstance.Value + "_updated"));
    }

    [Test]
    public void TestRepositoryUpdate_ShouldBeAbleToUpdateToNewValueWithoutRetrievingData()
    {
        var repository = _serviceProvider.GetService<IRepository>();

        Assert.That(repository, Is.Not.Null);

        var testInstance = new TestEntity
        {
            Value = Guid.NewGuid().ToString()
        };

        repository!.Insert(testInstance);
        repository.CommitChanges();

        repository.Update<TestEntity>(_ => _.Id == testInstance.Id,
                                      _ => new TestEntity
                                      {
                                          Value = "_updated"
                                      });
        var dbContext = _serviceProvider.GetService<TestDbContext>();

        var storedInstance = dbContext!.Set<TestEntity>().FirstOrDefault(_ => _.Id == testInstance.Id);

        Assert.That(storedInstance, Is.Not.Null);

        Assert.That(storedInstance!.Value, Is.EqualTo("_updated"));
    }
}