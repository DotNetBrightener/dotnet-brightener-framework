using DotNetBrightener.DataAccess.Auditing.Tests.DbContexts;
using DotNetBrightener.DataAccess.EF.Internal;
using DotNetBrightener.DataAccess.Models.Auditing;
using DotNetBrightener.Plugins.EventPubSub;
using DotNetBrightener.TestHelpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Moq;
using Shouldly;
using System.Collections.Immutable;
using Xunit;
using Xunit.Abstractions;

namespace DotNetBrightener.DataAccess.Auditing.Tests;

public class Auditing_DbContext_Tests : MsSqlServerBaseXUnitTest
{
    private readonly ITestOutputHelper _testOutputHelper;

    public Auditing_DbContext_Tests(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public async Task AuditingService_ShouldTrackChangesFromDbContext()
    {
        var mockAuditTrailHandler = new Mock<IMockReceiveData>();

        // Arrange

        var host = await CreateTestHost((hostContext, services) =>
        {
            services.AddEventPubSubService();
            services.AddScoped<IMockReceiveData>(p => mockAuditTrailHandler.Object);
            services.AddScoped<IEventHandler, HandleAuditTrail>();
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

        insertedEntityId.ShouldNotBe(0);

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

                                 entity.ShouldNotBeNull();

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

                             entity.ShouldNotBeNull();

                             dbContext.Remove(entity!);

                             await dbContext.SaveChangesAsync();
                         });

        await Task.Delay(500);

        // Assert
        mockAuditTrailHandler.Verify(x => x.ChangedProperties(It.IsAny<ImmutableList<AuditProperty>>()));

        // TODO: Implement the following assertion
        changedProperties.Select(x => x.PropertyName)
                         .ToList()
                         .ShouldBeInOrder(SortDirection.Ascending);

        // Clean up
        await WithScoped(host,
                         async dbContext =>
                         {
                             await dbContext.Database.EnsureDeletedAsync();
                         });
        await host.StopAsync();
    }

    private async Task<IHost> CreateTestHost(Action<HostBuilderContext, IServiceCollection> configureServices = null)
    {
        return XUnitTestHost.CreateTestHost(_testOutputHelper,
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
                                       (servicesProvider, options) =>
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