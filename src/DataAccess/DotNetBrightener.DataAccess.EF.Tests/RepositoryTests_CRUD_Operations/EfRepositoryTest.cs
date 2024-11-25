using DotNetBrightener.DataAccess.Exceptions;
using DotNetBrightener.DataAccess.Services;
using DotNetBrightener.TestHelpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using Newtonsoft.Json;
using Testcontainers.MsSql;
using Xunit;
using Xunit.Abstractions;

namespace DotNetBrightener.DataAccess.EF.Tests.RepositoryTests_CRUD_Operations;

public class EfRepositoryTest : MsSqlServerBaseXUnitTest
{
    private          CancellationTokenSource _cts;
    private          Mock<IMockAwaiter>      _mockAwaiter;
    private readonly ITestOutputHelper       _testOutputHelper;

    public EfRepositoryTest(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public async Task InsertMany_ShouldExecuteSuccessfully()
    {
        var host = ConfigureServices();
        await host.StartAsync();

        await InsertFakeData(host);

        using (var serviceScope = host.Services.CreateScope())
        {
            var serviceProvider = serviceScope.ServiceProvider;
            var repository      = serviceProvider.GetRequiredService<IRepository>();

            var deletableEntityCount = await repository.CountAsync<TestEntity>();

            deletableEntityCount.Should().Be(10);
        }

        await host.StopAsync();
    }


    [Fact]
    public async Task InsertOne_ShouldExecuteSuccessfully()
    {
        var host = ConfigureServices((services) =>
        {
        });

        await host.StartAsync();

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

        while (!_cts.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromSeconds(3));
        }

        await Task.Delay(TimeSpan.FromSeconds(3));
        _mockAwaiter.Verify(x => x.WaitFinished(It.Is<TestEntity>(x => x.Name.EndsWith("_Created by create event handler"))));

        await host.StopAsync();
    }


    [Fact]
    public async Task Update_ShouldExecuteSuccessfully()
    {
        var host = ConfigureServices((services) =>
        {
        });

        await host.StartAsync();

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

        while (!_cts.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromSeconds(3));
        }

        await Task.Delay(TimeSpan.FromSeconds(3));

        _mockAwaiter.Verify(x => x.WaitFinished(It.Is<TestEntity>(x => x.Name.Equals("Name1_Updated_Updated by update event handler"))));

        await host.StopAsync();
    }


    [Fact]
    public async Task UpdateWithDto_ShouldExecuteSuccessfully()
    {
        var host = ConfigureServices((services) =>
        {
        });

        await host.StartAsync();

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

        await Task.Delay(TimeSpan.FromSeconds(3));

        _mockAwaiter.Verify(x => x.WaitFinished(It.Is<TestEntity>(x => x.Name.Equals("Name1_Updated_From_Logic, _Updated by update event handler"))));

        await host.StopAsync();
    }


    [Fact]
    public async Task UpdateWithDto_UsingIgnore_ShouldExecuteSuccessfully()
    {
        var host = ConfigureServices((services) =>
        {
        });

        await host.StartAsync();

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

        await Task.Delay(TimeSpan.FromSeconds(3));
        
        _mockAwaiter.Verify(x => x.WaitFinished(It.Is<TestEntity>(x =>
                                                                      x.Name
                                                                       .Equals("Name1_Updated_From_Logic, _Updated by update event handler") &&
                                                                      x.Description
                                                                       .Equals("Original Description")
                                                                 )));


        await host.StopAsync();
    }


    [Fact]
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

            affectedRecords.Should().Be(3);
        }

        using (var serviceScope = host.Services.CreateScope())
        {
            var serviceProvider = serviceScope.ServiceProvider;
            var repository      = serviceProvider.GetRequiredService<IRepository>();

            var updatedEntities = repository.Fetch<TestEntity>(e => e.Name.StartsWith("Already Updated"))
                                            .ToArray();

            var index = 1;

            foreach (var record in updatedEntities)
            {
                record.Should().NotBeNull();
                record.Name.Should().Be($"Already Updated {index++}");
            }
        }
    }

    [Fact]
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

            record.Should().NotBeNull();
            record!.IsDeleted.Should().Be(true);
        }
    }

    [Fact]
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

    [Fact]
    public async Task DeleteMany_ShouldExecuteSuccessfully()
    {
        var host = ConfigureServices();

        await InsertFakeData(host);

        using (var serviceScope = host.Services.CreateScope())
        {
            var serviceProvider = serviceScope.ServiceProvider;
            var repository      = serviceProvider.GetRequiredService<IRepository>();

            var affectedRecords = await repository.DeleteManyAsync<TestEntity>(x => x.Name != "Name1", "Test deletion");

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

    private IHost ConfigureServices(Action<IServiceCollection> configureServices = null)
    {
        _cts         = new CancellationTokenSource();
        _mockAwaiter = new Mock<IMockAwaiter>();
        _mockAwaiter.Setup(x => x.WaitFinished(It.IsAny<object>()))
                    .Callback<object>((calledData) =>
                     {
                         _testOutputHelper.WriteLine("MockAwaiter called with data: ");
                         _testOutputHelper.WriteLine(JsonConvert.SerializeObject(calledData, Formatting.Indented));
                         _testOutputHelper.WriteLine("\r\n-----\r\n");

                         _cts.Cancel();
                     });

        var host = XUnitTestHost.CreateTestHost(_testOutputHelper,
                                                (hostContext, services) =>
                                                {
                                                    Configure(hostContext, services);

                                                    configureServices?.Invoke(services);
                                                });


        using (var serviceScope = host.Services.CreateScope())
        {
            var serviceProvider = serviceScope.ServiceProvider;

            using (var dbContext = serviceProvider.GetRequiredService<TestDbContext>())
            {
                dbContext.Database.EnsureCreated();
            }
        }

        return host;
    }

    private void Configure(HostBuilderContext hostContext, IServiceCollection services)
    {
        services.AddSingleton<IMockAwaiter>(_mockAwaiter.Object);

        var connectionString = MsSqlContainer.GetConnectionString($"MsSqlServerBaseTest");

        services
           .AddEFCentralizedDataServices<TestDbContext>(new DatabaseConfiguration
                                                        {
                                                            ConnectionString = connectionString,
                                                            DatabaseProvider =
                                                                DatabaseProvider.MsSql
                                                        },
                                                        hostContext.Configuration,
                                                        optionsBuilder =>
                                                        {
                                                            optionsBuilder
                                                               .UseSqlServer(connectionString,
                                                                             c =>
                                                                             {
                                                                                 c.EnableRetryOnFailure(20);
                                                                             });
                                                        });
        services.AddEventPubSubService()
                .AddEventHandlersFromAssemblies();


        services.AddSingleton(services);
    }
}