using DotNetBrightener.DataAccess.Auditing.Tests.DbContexts;
using DotNetBrightener.DataAccess.EF.Internal;
using DotNetBrightener.DataAccess.Models.Auditing;
using DotNetBrightener.DataAccess.Services;
using DotNetBrightener.Plugins.EventPubSub;
using DotNetBrightener.TestHelpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Immutable;
using Xunit;
using Xunit.Abstractions;

namespace DotNetBrightener.DataAccess.Auditing.Tests;

public class Auditing_Repository_Tests : MsSqlServerBaseXUnitTest
{
    private readonly ITestOutputHelper _testOutputHelper;

    public Auditing_Repository_Tests(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public async Task AuditingService_UseExpression_ShouldTrackChangesFromRepository()
    {
        long insertedEntityId = 0;
        var  insertedDateTime = DateTimeOffset.UtcNow;
        {
            var host = await CreateTestHost();

            // Acts
            await host.StartAsync();


            await WithScoped(host,
                             async (dbContext, repository, serviceProvider) =>
                             {
                                 await dbContext.Database.EnsureCreatedAsync();
                                 var entity = new TestEntity()
                                 {
                                     Name                = "John Smith",
                                     DateTimeOffsetValue = insertedDateTime,
                                     IntValue            = 10
                                 };

                                 await repository.InsertAsync(entity);
                                 await repository.CommitChangesAsync();

                                 insertedEntityId = entity.Id;
                             });

            insertedEntityId.Should().NotBe(0);
            await host.StopAsync();
        }

        await UpdateUsingExpression_ShouldOutputAuditEntry(insertedEntityId);
        await DeleteUsingExpression_ShouldOutputAuditEntry(insertedEntityId);

        //// Assert
        //mockAuditTrailHandler.Verify(x => x.ReceiveData(It.IsAny<AuditTrailMessage>()), Times.Exactly(3));
        //mockAuditTrailHandler.Verify(x => x.ChangedProperties(It.IsAny<ImmutableList<AuditProperty>>()),
        //                             Times.Exactly(3));

        //var expectedPropNamesReturned = new List<string>
        //{
        //    nameof(TestEntity.Name),
        //    nameof(TestEntity.BooleanValue),
        //    nameof(TestEntity.IntValue),
        //    nameof(TestEntity.DateTimeOffsetValue),
        //};

        //changedProperties.Select(x => x.PropertyName)
        //                 .OrderBy(x => x)
        //                 .ToList()
        //                 .Should()
        //                 .BeEquivalentTo(expectedPropNamesReturned.OrderBy(x => x).ToList());

        //initializedScopes.Count.Should().Be(3);
    }

    private async Task UpdateUsingExpression_ShouldOutputAuditEntry(long insertedEntityId)
    {
        var                          initializedScopes     = new List<Guid>();
        var                          mockAuditTrailHandler = new Mock<IMockReceiveData>();
        ImmutableList<AuditProperty> changedProperties     = ImmutableList<AuditProperty>.Empty;
        ILogger?                     logger                = null;

        DateTimeOffset? start0 = null;
        DateTimeOffset? start2 = null;

        CancellationTokenSource cts = new();

        mockAuditTrailHandler.Setup(x => x.ChangedProperties(It.IsAny<ImmutableList<AuditProperty>>()))
                             .Callback<ImmutableList<AuditProperty>>(cp =>
                              {
                                  start2 = DateTimeOffset.UtcNow;
                                  changedProperties = cp;

                                  logger?.LogInformation($"Hit ChangedProperties: {start2}");

                                  Task.Delay(100).Wait();
                                  cts.Cancel();
                              });

        var host = await CreateTestHost((hostContext, services) =>
        {
            services.AddEventPubSubService();
            services.AddScoped<IMockReceiveData>(p => mockAuditTrailHandler.Object);
            services.AddScoped<IEventHandler, HandleAuditTrail>();
        });

        await host.StartAsync();

        await WithScoped(host,
                         async (dbContext, repository, serviceProvider) =>
                         {
                             logger = serviceProvider.GetRequiredService<ILogger<Auditing_Repository_Tests>>();

                             start0 = DateTimeOffset.UtcNow;

                             logger.LogInformation($"Start test: {start0}");

                             await repository.DeleteOneAsync<TestEntity>(x => x.Id == insertedEntityId);
                         });

        while (!cts.Token.IsCancellationRequested)
        {
            await Task.Delay(100);
        }

        start2.Should().NotBeNull();
        
        logger.LogInformation($"Took {(start2 - start0)} to hit methods");
        
        mockAuditTrailHandler.Verify(x => x.ChangedProperties(It.IsAny<ImmutableList<AuditProperty>>()),
                                     Times.Once);

        await host.StopAsync();
    }

    private async Task DeleteUsingExpression_ShouldOutputAuditEntry(long insertedEntityId)
    {
        var                          initializedScopes     = new List<Guid>();
        var                          mockAuditTrailHandler = new Mock<IMockReceiveData>();
        ImmutableList<AuditProperty> changedProperties     = ImmutableList<AuditProperty>.Empty;
        ILogger?                     logger                = null;

        DateTimeOffset? start0 = null;
        DateTimeOffset? start2 = null;

        CancellationTokenSource cts = new();

        mockAuditTrailHandler.Setup(x => x.ChangedProperties(It.IsAny<ImmutableList<AuditProperty>>()))
                             .Callback<ImmutableList<AuditProperty>>(cp =>
                              {
                                  start2 = DateTimeOffset.UtcNow;
                                  changedProperties = cp;

                                  logger?.LogInformation($"Hit ChangedProperties: {start2}");

                                  Task.Delay(100).Wait();
                                  cts.Cancel();
                              });

        var host = await CreateTestHost((hostContext, services) =>
        {
            services.AddEventPubSubService();
            services.AddScoped<IMockReceiveData>(p => mockAuditTrailHandler.Object);
            services.AddScoped<IEventHandler, HandleAuditTrail>();
        });

        await host.StartAsync();

        await WithScoped(host,
                         async (dbContext, repository, serviceProvider) =>
                         {
                             logger = serviceProvider.GetRequiredService<ILogger<Auditing_Repository_Tests>>();

                             start0 = DateTimeOffset.UtcNow;

                             logger.LogInformation($"Start test: {start0}");

                             await repository.UpdateAsync<TestEntity>(x => x.Id == insertedEntityId,
                                                                      x => new TestEntity
                                                                      {
                                                                          Name = "John Smith Updated",
                                                                          DateTimeOffsetValue = DateTimeOffset.UtcNow.AddDays(1),
                                                                          IntValue = x.IntValue + 10,
                                                                          AnotherIntValue = x.IntValue + 20
                                                                      },
                                                                      expectedAffectedRows: 1);
                         });

        while (!cts.Token.IsCancellationRequested)
        {
            await Task.Delay(100);
        }

        start2.Should().NotBeNull();
        
        logger.LogInformation($"Took {(start2 - start0)} to hit methods");
        
        mockAuditTrailHandler.Verify(x => x.ChangedProperties(It.IsAny<ImmutableList<AuditProperty>>()),
                                     Times.Once);

        await host.StopAsync();
    }

    private async Task<IHost> CreateTestHost(Action<HostBuilderContext, IServiceCollection> configureServices = null)
    {
        return XUnitTestHost.CreateTestHost(testOutputHelper: _testOutputHelper,
                                            (hostContext, services) =>
                                            {
                                                ConfigureDataAccessService(services, hostContext);
                                                configureServices?.Invoke(hostContext, services);
                                            });
    }

    private void ConfigureDataAccessService(IServiceCollection services,
                                            HostBuilderContext hostContext)
    {
        services.TryAddSingleton<EFCoreExtendedServiceFactory>();

        services
           .AddEFCentralizedDataServices<
                TestAuditingDbContext>(new DatabaseConfiguration
                                       {
                                           ConnectionString = ConnectionString
                                       },
                                       hostContext.Configuration,
                                       options =>
                                       {
                                           options.UseSqlServer(ConnectionString,
                                                                s => s.EnableRetryOnFailure(20));
                                       });
    }

    private async Task WithScoped(IHost host, Func<TestAuditingDbContext, IRepository, IServiceProvider, Task> action)
    {
        using (var serviceScope = host.Services.CreateScope())
        {
            var serviceProvider = serviceScope.ServiceProvider;

            await using var dbContext  = serviceProvider.GetRequiredService<TestAuditingDbContext>();
            var             repository = serviceProvider.GetRequiredService<IRepository>();

            await action.Invoke(dbContext, repository, serviceProvider);
        }
    }
}