using DotNetBrightener.DataAccess.EF.Internal;
using DotNetBrightener.DataAccess.EF.Migrations;
using DotNetBrightener.TestHelpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace DotNetBrightener.DataAccess.EF.Tests.DbContext_Tests_Config;

public class DbContextInitializationWithConfigurationTests(ITestOutputHelper testOutputHelper) : MsSqlServerBaseXUnitTest(testOutputHelper)
{
    [Fact]
    public async Task Configurator_ShouldBeExecuted()
    {
        var mockConfigurator = new Mock<IDbContextConfigurator>();

        mockConfigurator.Setup(x => x.OnConfiguring(It.IsAny<DbContextOptionsBuilder>()))
                        .Verifiable();

        // Arrange
        var builder = new HostBuilder();
        ExtendedServiceFactory.ApplyServiceProviderFactory(builder);

        builder.ConfigureServices((hostContext, services) =>
        {
            services.TryAddSingleton<EFCoreExtendedServiceFactory>();
            services.AddTransient<IDbContextConfigurator>((p) => mockConfigurator.Object);

            services.AddDbContext<DbContextWithDynamicConfiguration>(c =>
            {
                c.UseSqlServer(ConnectionString);
            });
        });


        var host = builder.Build();

        // Acts
        await host.StartAsync();

        using (var serviceScope = host.Services.CreateScope())
        {
            var serviceProvider = serviceScope.ServiceProvider;

            await using (var dbContext = serviceProvider.GetRequiredService<DbContextWithDynamicConfiguration>())
            {
                await dbContext.Database.EnsureCreatedAsync();
            }
        }

        // Assert
        mockConfigurator.Verify(x => x.OnConfiguring(It.IsAny<DbContextOptionsBuilder>()), Times.Once);


        // Cleanup 
        using (var serviceScope = host.Services.CreateScope())
        {
            var serviceProvider = serviceScope.ServiceProvider;

            await using (var dbContext = serviceProvider.GetRequiredService<DbContextWithDynamicConfiguration>())
            {
                await dbContext.Database.EnsureDeletedAsync();
            }
        }

        await host.StopAsync();
    }
}