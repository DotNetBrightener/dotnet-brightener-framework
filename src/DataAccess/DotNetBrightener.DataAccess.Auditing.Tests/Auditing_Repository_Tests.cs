using DotNetBrightener.DataAccess.Auditing.Tests.DbContexts;
using DotNetBrightener.DataAccess.EF.Internal;
using DotNetBrightener.DataAccess.Models.Auditing;
using DotNetBrightener.DataAccess.Services;
using DotNetBrightener.Plugins.EventPubSub;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Moq;
using System.Collections.Immutable;
using DotNetBrightener.DataAccess.EF.Auditing;
using DotNetBrightener.TestHelpers;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace DotNetBrightener.DataAccess.Auditing.Tests;

public class Auditing_Repository_Tests(ITestOutputHelper testOutputHelper) : MsSqlServerBaseXUnitTest
{
    [Fact]
    public async Task AuditingService_UseExpression_ShouldTrackChangesFromRepository()
    {
        long insertedEntityId = 0;
        var  insertedDateTime = DateTimeOffset.UtcNow;
        {
            var host = CreateTestHost();

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

        //await WithScoped(host,
        //                 async (dbContext, repository) =>
        //                 {
        //                     var entity = await dbContext.Set<TestEntity>().FindAsync(insertedEntityId);

        //                     entity.Should().NotBeNull();

        //                     dbContext.Remove(entity!);

        //                     await dbContext.SaveChangesAsync();
        //                 });

        //await Task.Delay(500);

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


        // clean up
        {
            var host = CreateTestHost();

            // Acts
            await host.StartAsync();

            // Clean up
            await WithScoped(host,
                             async (dbContext, repository, serviceProvider) =>
                             {
                                 // await dbContext.Database.EnsureDeletedAsync();
                             });

            await host.StopAsync();
        }
    }

    private async Task UpdateUsingExpression_ShouldOutputAuditEntry(long insertedEntityId)
    {
        var                          initializedScopes     = new List<Guid>();
        var                          mockAuditTrailHandler = new Mock<IMockReceiveData>();
        ImmutableList<AuditProperty> changedProperties     = ImmutableList<AuditProperty>.Empty;
        ILogger?                     logger                = null;

        DateTimeOffset? start0 = null;
        DateTimeOffset? start1 = null;
        DateTimeOffset? start2 = null;

        CancellationTokenSource cts = new();

        mockAuditTrailHandler.Setup(x => x.ReceiveData(It.IsAny<AuditTrailMessage>()))
                             .Callback<AuditTrailMessage>(data =>
                              {
                                  start1 = DateTimeOffset.UtcNow;

                                  var scopes = data.AuditEntities
                                                   .Select(x => x.ScopeId)
                                                   .Distinct();

                                  initializedScopes.AddRange(scopes);
                                  initializedScopes = initializedScopes.Distinct()
                                                                       .ToList();

                                  foreach (var auditEntity in data.AuditEntities)
                                  {
                                      testOutputHelper.WriteLine(auditEntity.DebugView);
                                      testOutputHelper.WriteLine(auditEntity.EntityIdentifier);
                                  }

                                  logger?.LogInformation($"Hit ReceiveData: {start1}");
                              });

        mockAuditTrailHandler.Setup(x => x.ChangedProperties(It.IsAny<ImmutableList<AuditProperty>>()))
                             .Callback<ImmutableList<AuditProperty>>(cp =>
                              {
                                  start2 = DateTimeOffset.UtcNow;
                                  changedProperties = cp;

                                  logger?.LogInformation($"Hit ChangedProperties: {start2}");

                                  Task.Delay(100).Wait();
                                  cts.Cancel();
                              });

        var host = CreateTestHost((hostContext, services) =>
        {
            services.AddEventPubSubService();
            services.AddScoped<IMockReceiveData>(p => mockAuditTrailHandler.Object);
            services.AddScoped<IEventHandler, HandleAuditTrail>();
            services.AddAuditingService();
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

        start1.Should().NotBeNull();
        start2.Should().NotBeNull();
        
        logger.LogInformation($"Took {(start2 - start0)} to hit methods");
        
        mockAuditTrailHandler.Verify(x => x.ChangedProperties(It.IsAny<ImmutableList<AuditProperty>>()),
                                     Times.Once);

        foreach (var changedProperty in changedProperties)
        {
            logger.LogInformation(@"Property: {PropertyName}: 
    - Old value: {OldValue}. 
    - New value: {NewValue}.",
                                  changedProperty.PropertyName,
                                  changedProperty.OldValue,
                                  changedProperty.NewValue
                                 );
        }

        await host.StopAsync();
    }

    private IHost CreateTestHost(Action<HostBuilderContext, IServiceCollection> configureServices = null)
    {
        return XUnitTestHost.CreateTestHost(testOutputHelper,
                                            (hostContext, services) =>
                                            {
                                                ConfigureDataAccessService( services, hostContext);
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