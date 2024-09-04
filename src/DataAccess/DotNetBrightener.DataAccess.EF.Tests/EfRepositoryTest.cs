using DotNetBrightener.DataAccess.Exceptions;
using DotNetBrightener.DataAccess.Services;
using DotNetBrightener.TestHelpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;

namespace DotNetBrightener.DataAccess.EF.Tests;

internal class EfRepositoryTest : MsSqlServerBaseNUnitTest
{
    [TearDown]
    public async Task TearDown()
    {
        await TearDownHost();
    }

    [Test]
    public async Task InsertMany_ShouldExecuteSuccessfully()
    {
        var host = ConfigureServices();

        await InsertFakeData(host);

        using (var serviceScope = host.Services.CreateScope())
        {
            var serviceProvider = serviceScope.ServiceProvider;
            var repository      = serviceProvider.GetRequiredService<IRepository>();

            var deletableEntityCount = await repository.CountAsync<TestEntity>();

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

            await repository.InsertAsync(new TestEntity
            {
                Name = "Name1"
            });
            await repository.CommitChangesAsync();
        }

        // give the event handler some time to execute
        await Task.Delay(TimeSpan.FromSeconds(3));

        using (var serviceScope = host.Services.CreateScope())
        {
            var serviceProvider = serviceScope.ServiceProvider;
            var repository      = serviceProvider.GetRequiredService<IRepository>();

            var firstEntity = await repository.GetFirstAsync<TestEntity>(_ => true);
            firstEntity.Should().NotBeNull();
            firstEntity!.Name.Should().Be("Name1_Updated by event handler");
        }
    }


    [Test]
    public async Task Update_ShouldExecuteSuccessfully()
    {
        var host = ConfigureServices((services) =>
        {
        });

        using (var serviceScope = host.Services.CreateScope())
        {
            var serviceProvider = serviceScope.ServiceProvider;
            var repository      = serviceProvider.GetRequiredService<IRepository>();

            await repository.InsertAsync(new TestEntity
            {
                Name = "Name1"
            });
            await repository.CommitChangesAsync();
        }

        using (var serviceScope = host.Services.CreateScope())
        {
            var serviceProvider = serviceScope.ServiceProvider;
            var repository      = serviceProvider.GetRequiredService<IRepository>();

            var firstEntity = await repository.GetFirstAsync<TestEntity>(_ => true);
            firstEntity!.Name = "Name1_Updated";
            await repository.UpdateAsync(firstEntity);
            await repository.CommitChangesAsync();
        }

        // give the event handler some time to execute
        await Task.Delay(TimeSpan.FromSeconds(3));


        using (var serviceScope = host.Services.CreateScope())
        {
            var serviceProvider = serviceScope.ServiceProvider;
            var repository      = serviceProvider.GetRequiredService<IRepository>();

            var firstEntity = await repository.GetFirstAsync<TestEntity>(_ => true);
            Assert.That(firstEntity!.Name, Is.EqualTo("Name1_Updated_Updated by update event handler"));
        }
    }


    [Test]
    public async Task UpdateWithDto_ShouldExecuteSuccessfully()
    {
        var host = ConfigureServices((services) =>
        {
        });

        using (var serviceScope = host.Services.CreateScope())
        {
            var serviceProvider = serviceScope.ServiceProvider;
            var repository      = serviceProvider.GetRequiredService<IRepository>();

            await repository.InsertAsync(new TestEntity
            {
                Name = "Name1"
            });
            await repository.CommitChangesAsync();
        }

        using (var serviceScope = host.Services.CreateScope())
        {
            var serviceProvider = serviceScope.ServiceProvider;
            var repository      = serviceProvider.GetRequiredService<IRepository>();

            var firstEntity = await repository.GetFirstAsync<TestEntity>(_ => true);

            await repository.UpdateAsync(firstEntity!,
                                         new
                                         {
                                             Name = "Name1_Updated_From_Logic, "
                                         });
            await repository.CommitChangesAsync();
        }

        Thread.Sleep(TimeSpan.FromSeconds(2));

        using (var serviceScope = host.Services.CreateScope())
        {
            var serviceProvider = serviceScope.ServiceProvider;
            var repository      = serviceProvider.GetRequiredService<IRepository>();

            var firstEntity = await repository.GetFirstAsync<TestEntity>(_ => true);
            
            firstEntity!.Name.Should().Be("Name1_Updated_From_Logic, _Updated by update event handler");
        }
    }


    [Test]
    public async Task UpdateWithDto_UsingIgnore_ShouldExecuteSuccessfully()
    {
        var host = ConfigureServices((services) =>
        {
        });

        using (var serviceScope = host.Services.CreateScope())
        {
            var serviceProvider = serviceScope.ServiceProvider;
            var repository      = serviceProvider.GetRequiredService<IRepository>();

            await repository.InsertAsync(new TestEntity
            {
                Name        = "Name1",
                Description = "Original Description"
            });
            await repository.CommitChangesAsync();
        }

        using (var serviceScope = host.Services.CreateScope())
        {
            var serviceProvider = serviceScope.ServiceProvider;
            var repository      = serviceProvider.GetRequiredService<IRepository>();

            var firstEntity = await repository.GetFirstAsync<TestEntity>(_ => true);

            firstEntity.Should().NotBeNull();

            await repository.UpdateAsync(firstEntity!,
                                         new
                                         {
                                             Name        = "Name1_Updated_From_Logic, ",
                                             Description = "Description_Updated_From_Logic"
                                         },
                                         nameof(firstEntity.Description));
            await repository.CommitChangesAsync();
        }

        Thread.Sleep(TimeSpan.FromSeconds(2));

        using (var serviceScope = host.Services.CreateScope())
        {
            var serviceProvider = serviceScope.ServiceProvider;
            var repository      = serviceProvider.GetRequiredService<IRepository>();

            var firstEntity = await repository.GetFirstAsync<TestEntity>(_ => true);
            firstEntity!.Name.Should().Be("Name1_Updated_From_Logic, _Updated by update event handler");
            firstEntity.Description.Should().Be("Original Description");
        }
    }


    [Test]
    public async Task UpdateMany_ShouldExecuteSuccessfully()
    {
        var host = ConfigureServices((services) =>
        {
        });

        using (var serviceScope = host.Services.CreateScope())
        {
            var serviceProvider = serviceScope.ServiceProvider;
            var repository      = serviceProvider.GetRequiredService<IRepository>();

            await repository.InsertAsync(new TestEntity
            {
                Name = "Name 1"
            });
            await repository.InsertAsync(new TestEntity
            {
                Name = "Name 2"
            });
            await repository.InsertAsync(new TestEntity
            {
                Name = "Name 3"
            });
            await repository.InsertAsync(new TestEntity
            {
                Name = "Name 4"
            });
            await repository.InsertAsync(new TestEntity
            {
                Name = "Name 5"
            });
            await repository.InsertAsync(new TestEntity
            {
                Name = "To update 1"
            });
            await repository.InsertAsync(new TestEntity
            {
                Name = "To update 2"
            });
            await repository.InsertAsync(new TestEntity
            {
                Name = "To update 3"
            });
            await repository.CommitChangesAsync();
        }

        using (var serviceScope = host.Services.CreateScope())
        {
            var serviceProvider = serviceScope.ServiceProvider;
            var repository      = serviceProvider.GetRequiredService<IRepository>();

            var affectedRecords =
                await repository.UpdateAsync<TestEntity>(entity => entity.Name.StartsWith("To update"),
                                                         entity => new TestEntity
                                                         {
                                                             Name = entity
                                                                   .Name.Replace("_Updated by event handler", "")
                                                                   .Replace("To update", "Already Updated")
                                                         });

            Assert.That(affectedRecords, Is.EqualTo(3));
        }

        using (var serviceScope = host.Services.CreateScope())
        {
            var serviceProvider = serviceScope.ServiceProvider;
            var repository      = serviceProvider.GetRequiredService<IRepository>();

            var updatedEntities = repository.Fetch<TestEntity>(e => e.Name.StartsWith("Already Updated"))
                                            .ToArray();

            int index = 1;

            foreach (var record in updatedEntities)
            {
                Assert.That(record, Is.Not.Null);
                Assert.That(record.Name, Is.EqualTo($"Already Updated {index++}"));
            }
        }
    }

    [Test]
    public async Task DeleteOne_ShouldExecuteSuccessfully()
    {
        var host = ConfigureServices();

        await InsertFakeData(host);

        using (var serviceScope = host.Services.CreateScope())
        {
            var serviceProvider = serviceScope.ServiceProvider;
            var repository      = serviceProvider.GetRequiredService<IRepository>();

            await repository.DeleteOneAsync<TestEntity>(x => x.Name == "Name1");
        }

        using (var serviceScope = host.Services.CreateScope())
        {
            var serviceProvider = serviceScope.ServiceProvider;
            var repository      = serviceProvider.GetRequiredService<IRepository>();

            var record = await repository.GetFirstAsync<TestEntity>(x => x.Name == "Name1");

            Assert.That(record, Is.Not.Null);
            Assert.That(record.IsDeleted, Is.EqualTo(true));
        }
    }

    [Test]
    public async Task DeleteOne_ShouldExecute_Not_Successfully()
    {
        var host = ConfigureServices();

        await InsertFakeData(host);
        await InsertFakeData(host);

        Action act = () =>
        {
            using (var serviceScope = host.Services.CreateScope())
            {
                var serviceProvider = serviceScope.ServiceProvider;
                var repository      = serviceProvider.GetRequiredService<IRepository>();

                repository.DeleteOneAsync<TestEntity>(x => x.Name == "Name1").Wait();
            }
        };

        act.Should()
           .Throw<ExpectedAffectedRecordMismatchException>()
           .Where(ex => ex.ExpectedAffectedRecords == 1 && ex.ActualAffectedRecords == 2);

        using (var serviceScope = host.Services.CreateScope())
        {
            var serviceProvider = serviceScope.ServiceProvider;
            var repository      = serviceProvider.GetRequiredService<IRepository>();

            var record = await repository.Fetch<TestEntity>(x => x.Name == "Name1" && !x.IsDeleted)
                                         .CountAsync();

            record.Should().Be(2, "The transaction should roll back.");
        }
    }

    [Test]
    public async Task DeleteMany_ShouldExecuteSuccessfully()
    {
        var host = ConfigureServices();

        await InsertFakeData(host);

        using (var serviceScope = host.Services.CreateScope())
        {
            var serviceProvider = serviceScope.ServiceProvider;
            var repository      = serviceProvider.GetRequiredService<IRepository>();

            int affectedRecords = await repository.DeleteManyAsync<TestEntity>(x => x.Name != "Name1", "Test deletion");

            affectedRecords.Should().Be(9);
        }

        using (var serviceScope = host.Services.CreateScope())
        {
            var serviceProvider = serviceScope.ServiceProvider;
            var repository      = serviceProvider.GetRequiredService<IRepository>();

            var records = repository.Fetch<TestEntity>(x => x.Name != "Name1");

            foreach (var record in records)
            {
                record.IsDeleted.Should().BeTrue();
                record.DeletionReason.Should().Be("Test deletion");
            }
        }
    }

    private static async Task InsertFakeData(IHost host)
    {
        using (var serviceScope = host.Services.CreateScope())
        {
            var serviceProvider = serviceScope.ServiceProvider;
            var repository      = serviceProvider.GetRequiredService<IRepository>();

            var entities = new List<TestEntity>
            {
                new TestEntity
                {
                    Name = "Name1"
                },
                new TestEntity
                {
                    Name = "Name2"
                },
                new TestEntity
                {
                    Name = "Name3"
                },
                new TestEntity
                {
                    Name = "Name4"
                },
                new TestEntity
                {
                    Name = "Name5"
                },
                new TestEntity
                {
                    Name = "Name6"
                },
                new TestEntity
                {
                    Name = "Name7"
                },
                new TestEntity
                {
                    Name = "Name8"
                },
                new TestEntity
                {
                    Name = "Name9"
                },
                new TestEntity
                {
                    Name = "Name10"
                }
            };

            await repository.BulkInsertAsync(entities);
            await repository.CommitChangesAsync();
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
                services.AddEFCentralizedDataServices<TestDbContext>(new DatabaseConfiguration
                                                                     {
                                                                         ConnectionString = ConnectionString,
                                                                         DatabaseProvider = DatabaseProvider.MsSql
                                                                     },
                                                                     hostContext.Configuration,
                                                                     optionsBuilder =>
                                                                     {
                                                                         optionsBuilder
                                                                            .UseSqlServer(ConnectionString,
                                                                                          c =>
                                                                                          {
                                                                                              c.EnableRetryOnFailure(20);
                                                                                          });
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

    private async Task TearDownHost()
    {
        var builder = new HostBuilder()
           .ConfigureServices((hostContext, serviceCollection) =>
            {
                serviceCollection.AddDbContext<TestDbContext>(options =>
                {
                    options.UseSqlServer(ConnectionString);
                });
            });

        var host = builder.Build();

        using var serviceScope    = host.Services.CreateScope();
        var       serviceProvider = serviceScope.ServiceProvider;

        await using var dbContext = serviceProvider.GetRequiredService<TestDbContext>();
        await dbContext.Database.EnsureDeletedAsync();
    }
}