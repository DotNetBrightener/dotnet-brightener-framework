using DotNetBrightener.DataAccess.Auditing.Tests.DbContexts;
using DotNetBrightener.DataAccess.EF.Auditing;
using DotNetBrightener.DataAccess.EF.Internal;
using DotNetBrightener.DataAccess.Models.Auditing;
using DotNetBrightener.Plugins.EventPubSub;
using DotNetBrightener.TestHelpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Moq;
using System.Collections.Immutable;
using Xunit;
using Xunit.Abstractions;

namespace DotNetBrightener.DataAccess.Auditing.Tests;

public class Auditing_DbContext_Tests(ITestOutputHelper testOutputHelper) : MsSqlServerBaseXUnitTest
{
    [Fact]
    public async Task AuditingService_ShouldTrackChangesFromDbContext()
    {
        var initializedScopes     = new List<Guid>();
        var mockAuditTrailHandler = new Mock<IMockReceiveData>();

        mockAuditTrailHandler.Setup(x => x.ReceiveData(It.IsAny<AuditTrailMessage>()))
                             .Callback<AuditTrailMessage>(data =>
                              {
                                  var scopes = data.AuditEntities
                                                   .Select(x => x.ScopeId)
                                                   .Distinct();

                                  initializedScopes.AddRange(scopes);
                                  initializedScopes = initializedScopes.Distinct()
                                                                       .ToList();
                              });

        // Arrange

        var host = CreateTestHost((hostContext, services) =>
        {
            services.AddEventPubSubService();
            services.AddScoped<IMockReceiveData>(p => mockAuditTrailHandler.Object);
            services.AddScoped<IEventHandler, HandleAuditTrail>();
            services.AddAuditingService();
        });


        // Acts
        await host.StartAsync();


        await WithScoped(host,
                         async dbContext =>
                         {
                             await dbContext.Database.EnsureCreatedAsync();
                         });

        long insertedEntityId = 0;
        var  insertedDateTime = DateTimeOffset.UtcNow;

        await WithScoped(host,
                         async dbContext =>
                         {
                             var entity = new TestEntity()
                             {
                                 Name                = "John Smith",
                                 DateTimeOffsetValue = insertedDateTime,
                                 IntValue            = 10
                             };

                             dbContext.Add(entity);

                             await dbContext.SaveChangesAsync();

                             insertedEntityId = entity.Id;
                         });

        insertedEntityId.Should().NotBe(0);

        ImmutableList<AuditProperty> changedProperties = ImmutableList<AuditProperty>.Empty;
        
        {
            CancellationTokenSource      cts               = new();

            var changePropertyMethodMockSetup = mockAuditTrailHandler
               .Setup(x => x.ChangedProperties(It.IsAny<ImmutableList<AuditProperty>>()));

            changePropertyMethodMockSetup.Callback<ImmutableList<AuditProperty>>(cp =>
            {
                changedProperties = cp;
                cts.Cancel();
            });

            await WithScoped(host,
                             async dbContext =>
                             {
                                 var entity = await dbContext.Set<TestEntity>().FindAsync(insertedEntityId);

                                 entity.Should().NotBeNull();

                                 entity!.Name = "John Smith Updated";

                                 dbContext.Update(entity);

                                 await dbContext.SaveChangesAsync();
                             });

            while (!cts.IsCancellationRequested)
            {
                await Task.Delay(100);
            }

            mockAuditTrailHandler.Verify(x => x.ChangedProperties(It.IsAny<ImmutableList<AuditProperty>>()),
                                         Times.Exactly(2));
            
            changedProperties = ImmutableList<AuditProperty>.Empty;
        }

        await WithScoped(host,
                         async dbContext =>
                         {
                             var entity = await dbContext.Set<TestEntity>().FindAsync(insertedEntityId);

                             entity.Should().NotBeNull();

                             dbContext.Remove(entity!);

                             await dbContext.SaveChangesAsync();
                         });

        await Task.Delay(500);

        // Assert
        mockAuditTrailHandler.Verify(x => x.ReceiveData(It.IsAny<AuditTrailMessage>()), Times.Exactly(3));
        mockAuditTrailHandler.Verify(x => x.ChangedProperties(It.IsAny<ImmutableList<AuditProperty>>()),
                                     Times.Exactly(3));
        
        changedProperties.Select(x => x.PropertyName)
                         .ToList()
                         .Should()
                         .BeInAscendingOrder();

        initializedScopes.Count.Should().Be(3);


        // Clean up
        await WithScoped(host,
                         async dbContext =>
                         {
                             await dbContext.Database.EnsureDeletedAsync();
                         });
        await host.StopAsync();
    }

    private IHost CreateTestHost(Action<HostBuilderContext, IServiceCollection> configureServices = null)
    {
        return XUnitTestHost.CreateTestHost(testOutputHelper,
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

    private async Task WithScoped(IHost host, Func<TestAuditingDbContext, Task> action)
    {
        using (var serviceScope = host.Services.CreateScope())
        {
            var serviceProvider = serviceScope.ServiceProvider;

            await using var dbContext = serviceProvider.GetRequiredService<TestAuditingDbContext>();

            await action.Invoke(dbContext);
        }
    }
}