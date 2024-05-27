using DotNetBrightener.DataAccess.Events;
using DotNetBrightener.DataAccess.Services;
using DotNetBrightener.Plugins.EventPubSub;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using System.Reflection;

namespace DotNetBrightener.DataAccess.EF.Tests;

public class TestEntityOnCreatedEventHandler: IEventHandler<EntityCreated<TestEntity>>
{
    public int Priority => 100;
    private readonly TestDbContext _dbContext;

    public TestEntityOnCreatedEventHandler(TestDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> HandleEvent(EntityCreated<TestEntity> eventMessage)
    {
        Assert.That(eventMessage.Entity, Is.Not.Null);

        eventMessage.Entity.Name = $"{eventMessage.Entity.Name}_Updated by event handler";

        _dbContext.Set<TestEntity>().Update(eventMessage.Entity);
        await _dbContext.SaveChangesAsync();

        return true;
    }
}


internal class EfRepositoryTest
{
    private string _connectionString;

    [SetUp]
    public void Setup()
    {
        _connectionString =
            $"Server=(localdb)\\MSSQLLocalDB;Database=DataAccess_UnitTest{DateTime.Now:yyyyMMddHHmm};Trusted_Connection=True;MultipleActiveResultSets=true";
        // _connectionString = $"Server=100.121.179.124;Database=DataMigration_UnitTest{DateTime.Now:yyyyMMddHHmm};User Id=sa;Password=sCpTXbW8jbSbbUpILfZVulTiwqcPyJWt;MultipleActiveResultSets=True;Encrypt=True;TrustServerCertificate=True;";
    }

    [TearDown]
    public void TearDown()
    {
        TearDownHost();
    }

    [Test]
    public async Task InsertMany_ShouldExecuteSuccessfully()
    {
        var host = ConfigureServices();

        InsertFakeData(host);

        using (var serviceScope = host.Services.CreateScope())
        {
            var serviceProvider = serviceScope.ServiceProvider;
            var repository      = serviceProvider.GetRequiredService<IRepository>();

            var deletableEntityCount = repository.Fetch<TestEntity>().Count();

            Assert.That(deletableEntityCount, Is.EqualTo(10));
        }
    }


    [Test]
    public async Task InsertOne_ShouldExecuteSuccessfully()
    {
        var host = ConfigureServices((services) =>
        {
        });

        using (var serviceScope = host.Services.CreateScope())
        {
            var serviceProvider = serviceScope.ServiceProvider;
            var repository      = serviceProvider.GetRequiredService<IRepository>();

            repository.Insert(new TestEntity { Name = "Name1" });
            repository.CommitChanges();
        }
        
        using (var serviceScope = host.Services.CreateScope())
        {
            var serviceProvider = serviceScope.ServiceProvider;
            var repository      = serviceProvider.GetRequiredService<IRepository>();

            var firstEntity = repository.GetFirst<TestEntity>(_ => true);
            Assert.That(firstEntity.Name, Is.EqualTo("Name1_Updated by event handler"));
        }


    }

    [Test]
    public async Task DeleteOne_ShouldExecuteSuccessfully()
    {
        var host = ConfigureServices();

        InsertFakeData(host);

        using (var serviceScope = host.Services.CreateScope())
        {
            var serviceProvider = serviceScope.ServiceProvider;
            var repository      = serviceProvider.GetRequiredService<IRepository>();

            repository.DeleteOne<TestEntity>(x => x.Name == "Name1");
        }

        using (var serviceScope = host.Services.CreateScope())
        {
            var serviceProvider = serviceScope.ServiceProvider;
            var repository      = serviceProvider.GetRequiredService<IRepository>();

            var record = repository.GetFirst<TestEntity>(x => x.Name == "Name1");
            
            Assert.That(record, Is.Not.Null);
            Assert.That(record.IsDeleted, Is.EqualTo(true));
        }
    }

    [Test]
    public async Task DeleteMany_ShouldExecuteSuccessfully()
    {
        var host = ConfigureServices();

        InsertFakeData(host);

        using (var serviceScope = host.Services.CreateScope())
        {
            var serviceProvider = serviceScope.ServiceProvider;
            var repository      = serviceProvider.GetRequiredService<IRepository>();

            int affectedRecords = repository.DeleteMany<TestEntity>(x => x.Name != "Name1", "Test deletion");
            Assert.That(affectedRecords, Is.EqualTo(9));
        }

        using (var serviceScope = host.Services.CreateScope())
        {
            var serviceProvider = serviceScope.ServiceProvider;
            var repository      = serviceProvider.GetRequiredService<IRepository>();

            var records = repository.Fetch<TestEntity>(x => x.Name != "Name1");

            foreach (var record in records)
            {
                Assert.That(record.IsDeleted, Is.EqualTo(true));
                Assert.That(record.DeletionReason, Is.EqualTo("Test deletion"));
            }
        }
    }

    private static void InsertFakeData(IHost host)
    {
        using (var serviceScope = host.Services.CreateScope())
        {
            var serviceProvider = serviceScope.ServiceProvider;
            var repository      = serviceProvider.GetRequiredService<IRepository>();

            var entities = new List<TestEntity>
            {
                new TestEntity {Name = "Name1"},
                new TestEntity {Name = "Name2"},
                new TestEntity {Name = "Name3"},
                new TestEntity {Name = "Name4"},
                new TestEntity {Name = "Name5"},
                new TestEntity {Name = "Name6"},
                new TestEntity {Name = "Name7"},
                new TestEntity {Name = "Name8"},
                new TestEntity {Name = "Name9"},
                new TestEntity {Name = "Name10"}
            };

            repository.InsertMany(entities);
            repository.CommitChanges();
        }
    }

    //[Test]
    //public async Task AddDataMigrator_ShouldThrowBecauseOfNotInitializeDataMigrationFirst()
    //{
    //    Assert.Throws(Is.TypeOf<InvalidOperationException>()
    //                    .And.Message
    //                    .EqualTo("The data migrations must be enabled first using EnableDataMigrations method"),
    //                  () =>
    //                  {
    //                      var builder = new HostBuilder()
    //                         .ConfigureServices((hostContext, services) =>
    //                          {
    //                              services.AddDataMigrator<GoodMigration>();
    //                          });

    //                      var host = builder.Build();
    //                  });
    //}

    private IHost ConfigureServices(Action<IServiceCollection> configureServices = null)
    {
        var builder = new HostBuilder()
           .ConfigureServices((hostContext, services) =>
            {
                services.AddEntityFrameworkDataServices<TestDbContext>(new DatabaseConfiguration
                                                                       {
                                                                           ConnectionString = _connectionString,
                                                                           DatabaseProvider = DatabaseProvider.MsSql
                                                                       },
                                                                       hostContext.Configuration,
                                                                       optionsBuilder =>
                                                                       {
                                                                           optionsBuilder
                                                                              .UseSqlServer(_connectionString);
                                                                       });

                configureServices?.Invoke(services);
            });

        builder.ConfigureServices((context, services) =>
        {
            services.AddEventPubSubService()
                    .AddEventHandlersFromAssemblies();
        });

        var host = builder.Build();


        using var serviceScope    = host.Services.CreateScope();
        var       serviceProvider = serviceScope.ServiceProvider;

        using var dbContext = serviceProvider.GetRequiredService<TestDbContext>();
        dbContext.Database.EnsureCreated();

        return host;
    }

    private void TearDownHost()
    {
        var builder = new HostBuilder()
           .ConfigureServices((hostContext, serviceCollection) =>
            {
                serviceCollection.AddDbContext<TestDbContext>(options =>
                {
                    options.UseSqlServer(_connectionString);
                });
            });

        var host = builder.Build();

        using var serviceScope    = host.Services.CreateScope();
        var       serviceProvider = serviceScope.ServiceProvider;

        using var dbContext = serviceProvider.GetRequiredService<TestDbContext>();
        dbContext.Database.EnsureDeleted();
    }
}