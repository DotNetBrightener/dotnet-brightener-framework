using DotNetBrightener.DataAccess.Auditing.EventMessages;
using DotNetBrightener.DataAccess.Auditing.Tests.DbContexts;
using DotNetBrightener.DataAccess.EF.Internal;
using DotNetBrightener.Plugins.EventPubSub;
using DotNetBrightener.TestHelpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Moq;
using Xunit;

namespace DotNetBrightener.DataAccess.Auditing.Tests;

public interface IMockReceiveData
{
    void ReceiveData(AuditTrailMessage data);
}

public class HandleAuditTrail(IMockReceiveData mocker) : IEventHandler<AuditTrailMessage>
{
    public async Task<bool> HandleEvent(AuditTrailMessage eventMessage)
    {
        mocker.ReceiveData(eventMessage);

        var auditEntries = eventMessage.AuditEntities;

        return true;
    }

    public int Priority => 10_000;
}

public class AuditingInitializationTests : MsSqlServerBaseXUnitTest
{
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


        await WithScoped(host,
                         async dbContext =>
                         {
                             var entity = new TestEntity()
                             {
                                 Name = "John Smith",
                                 DateTimeOffsetValue = DateTimeOffset.UtcNow
                             };
                             
                             dbContext.Add(entity);

                             await dbContext.SaveChangesAsync();
                         });


        await WithScoped(host,
                         async dbContext =>
                         {
                             var entity = dbContext.Set<TestEntity>()
                                                   .FirstOrDefault(x => x.Name == "John Smith");

                             entity.Should().NotBeNull();

                             entity!.Name = "John Smith Updated";
                             entity.IntValue += 10;

                             dbContext.Update(entity);

                             await dbContext.SaveChangesAsync();
                         });


        await WithScoped(host,
                         async dbContext =>
                         {
                             var entity = dbContext.Set<TestEntity>()
                                                   .FirstOrDefault(x => x.Name == "John Smith Updated");

                             entity.Should().NotBeNull();

                             dbContext.Remove(entity!);

                             await dbContext.SaveChangesAsync();
                         });

        await Task.Delay(500);

        // Assert
        mockAuditTrailHandler.Verify(x => x.ReceiveData(It.IsAny<AuditTrailMessage>()), Times.Exactly(3));
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