using DotNetBrightener.DataAccess.EF.Extensions;
using DotNetBrightener.DataAccess.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetBrightener.DataAccess.EF.Tests;

public class EfRepositoryTests
{
    private ServiceProvider _serviceProvider;

    private static readonly string DateTimeString = DateTime.Now.ToString("yyyyMMddHHmmss");

    private static readonly string ConnectionString = $"Data Source=192.168.20.163;Initial Catalog=dnb_testdb_{DateTimeString};User ID=sa;Password=sCpTXbW8jbSbbUpILfZVulTiwqcPyJWt;MultipleActiveResultSets=True;Encrypt=True;TrustServerCertificate=True;";

    [SetUp]
    public void Setup()
    {
        var serviceCollection = new ServiceCollection();

        serviceCollection.AddEntityFrameworkDataServices<TestDbContext>(new DatabaseConfiguration
                                                                        {
                                                                            ConnectionString = ConnectionString,
                                                                            DatabaseProvider = DatabaseProvider.MsSql
                                                                        },
                                                                        optionBuilder =>
                                                                        {
                                                                            optionBuilder
                                                                               .UseSqlServer(ConnectionString);
                                                                        });

        serviceCollection.AddLogging();
        serviceCollection.AddEventPubSubService();
        _serviceProvider = serviceCollection.BuildServiceProvider();

        var dbContext = _serviceProvider.GetService<TestDbContext>();
        dbContext!.Database.EnsureCreated();
    }

    [TearDown]
    public void TearDown()
    {
        var dbContext = _serviceProvider.GetService<TestDbContext>();
        dbContext!.Database.EnsureDeleted();
    }

    [Test]
    public void TestRepositoryUpdate_ShouldBeAbleToUpdateWithoutRetrievingData()
    {
        var repository = _serviceProvider.GetService<IRepository>();

        Assert.IsNotNull(repository);

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

        Assert.IsNotNull(storedInstance);

        Assert.That(storedInstance!.Value, Is.EqualTo(testInstance.Value + "_updated"));
    }

    [Test]
    public void TestRepositoryUpdate_ShouldBeAbleToUpdateToNewValueWithoutRetrievingData()
    {
        var repository = _serviceProvider.GetService<IRepository>();

        Assert.IsNotNull(repository);

        var testInstance = new TestEntity
        {
            Value = Guid.NewGuid().ToString()
        };

        repository!.Insert(testInstance);
        repository.CommitChanges();

        repository.Update<TestEntity>(_ => _.Id == testInstance.Id,
                                      new TestEntity
                                      {
                                          Value = "_updated"
                                      });
        var dbContext = _serviceProvider.GetService<TestDbContext>();

        var storedInstance = dbContext!.Set<TestEntity>().FirstOrDefault(_ => _.Id == testInstance.Id);

        Assert.IsNotNull(storedInstance);

        Assert.That(storedInstance!.Value, Is.EqualTo("_updated"));
    }
}