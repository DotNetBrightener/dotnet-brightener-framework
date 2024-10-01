using System.Collections.Immutable;
using DotNetBrightener.DataAccess.Auditing.Entities;
using DotNetBrightener.DataAccess.Auditing.EventMessages;
using DotNetBrightener.DataAccess.Auditing.Tests.DbContexts;
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
using Xunit;
using Xunit.Abstractions;

namespace DotNetBrightener.DataAccess.Auditing.Tests;

public interface IMockReceiveData
{
    void ReceiveData(AuditTrailMessage                  data);

    void ChangedProperties(ImmutableList<AuditProperty> auditEntityChangedAuditProperties);
}

public class HandleAuditTrail(IMockReceiveData mocker) : IEventHandler<AuditTrailMessage>
{
    public async Task<bool> HandleEvent(AuditTrailMessage eventMessage)
    {
        mocker.ReceiveData(eventMessage);

        var auditEntries = eventMessage.AuditEntities;

        foreach (var auditEntity in auditEntries)
        {
            mocker.ChangedProperties(auditEntity.ChangedAuditProperties);
        }

        return true;
    }

    public int Priority => 10_000;
}

public class AuditingInitializationTests(ITestOutputHelper testOutputHelper) : MsSqlServerBaseXUnitTest
{
    private readonly ITestOutputHelper _testOutputHelper = testOutputHelper;

    [Fact]
    public async Task Configurator_ShouldBeExecuted()
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

                                  foreach (var auditEntity in data.AuditEntities)
                                  {
                                      testOutputHelper.WriteLine(auditEntity.DebugView);
                                  }
                              });

        // Arrange
        var builder = new HostBuilder()
           .ConfigureServices((hostContext, services) =>
            {
                services.TryAddSingleton<EFCoreExtendedServiceFactory>();
                services.AddScoped<IMockReceiveData>(p => mockAuditTrailHandler.Object);
                services.AddScoped<IEventHandler, HandleAuditTrail>();

                services.AddEventPubSubService();

                services.AddDbContext<TestAuditingDbContext>(c =>
                {
                    c.UseSqlServer(ConnectionString);
                });

                services.AddAuditContext();
            });

        builder.UseServiceProviderFactory(new ExtendedServiceFactory());
        IHost host = builder.Build();

        // Acts
        await host.StartAsync();


        await WithScoped(host,
                         async dbContext =>
                         {
                             await dbContext.Database.EnsureCreatedAsync();
                         });

        long insertedEntityId = 0;
        var insertedDateTime = DateTimeOffset.UtcNow;

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

        var changePropertyMethodMockSetup = mockAuditTrailHandler
                                           .Setup(x => x.ChangedProperties(It.IsAny<ImmutableList<AuditProperty>>()));

        changePropertyMethodMockSetup.Callback<ImmutableList<AuditProperty>>(cp =>
        {
            changedProperties = cp;
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

        await Task.Delay(200);
        mockAuditTrailHandler.Verify(x => x.ChangedProperties(It.IsAny<ImmutableList<AuditProperty>>()), Times.Exactly(2));
        changedProperties.Count.Should().Be(1);
        changedProperties[0].PropertyName.Should().Be("Name");
        changedProperties[0].OldValue.Should().Be("John Smith");
        changedProperties[0].NewValue.Should().Be("John Smith Updated");

        changedProperties = ImmutableList<AuditProperty>.Empty;

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
        mockAuditTrailHandler.Verify(x => x.ChangedProperties(It.IsAny<ImmutableList<AuditProperty>>()), Times.Exactly(3));
        
        var expectedPropNamesReturned = new List<string>
        {
            nameof(TestEntity.Name),
            nameof(TestEntity.BooleanValue),
            nameof(TestEntity.IntValue),
            nameof(TestEntity.DateTimeOffsetValue),
        };

        changedProperties.Select(x => x.PropertyName)
                         .OrderBy(x => x)
                         .ToList()
                         .Should()
                         .BeEquivalentTo(expectedPropNamesReturned.OrderBy(x => x).ToList());

        initializedScopes.Count.Should().Be(3);


        // Clean up
        await WithScoped(host,
                         async dbContext =>
                         {
                             await dbContext.Database.EnsureDeletedAsync();
                         });
        await host.StopAsync();
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