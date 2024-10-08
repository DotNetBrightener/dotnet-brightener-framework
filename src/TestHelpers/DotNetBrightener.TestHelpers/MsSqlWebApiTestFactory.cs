using DotNet.Testcontainers.Builders;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MsSql;
using Xunit;
using Xunit.Sdk;

namespace DotNetBrightener.TestHelpers;

public class MsSqlWebApiTestFactory<TEndpoint> : WebApplicationFactory<TEndpoint>, IAsyncLifetime
    where TEndpoint : class
{
    protected MsSqlContainer MsSqlContainer;

    protected readonly string DatabaseName = $"WebApi_IntegrationTest_{DateTime.Now:yyyyMMddHHmm}";

    protected string ConnectionString;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(ConfigureTestServices);
    }

    protected virtual void ConfigureTestServices(IServiceCollection serviceCollection)
    {
    }

    protected virtual void ReplaceDbContextOption<TDbContextType>(IServiceCollection serviceCollection)
        where TDbContextType : DbContext
    {
        serviceCollection.Remove(serviceCollection.Single(a => typeof(DbContextOptions<TDbContextType>) ==
                                                               a.ServiceType));
        serviceCollection.AddDbContext<TDbContextType>(options =>
        {
            options.UseSqlServer(ConnectionString, c=>
            {
                c.EnableRetryOnFailure(3);
            });
        });
    }

    public virtual async Task InitializeAsync()
    {
        var currentTestingType = GetType().Name;

        var containerName = String.Concat("sqlserver-2022-", currentTestingType, $"-{Guid.NewGuid()}");

        MsSqlContainer = new MsSqlBuilder()
                        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
                        .WithPassword("Str0ng3stP@s5w0rd3ver!")
                        .WithName(containerName)
                        .WithWaitStrategy(Wait.ForUnixContainer()
                                              .UntilMessageIsLogged("SQL Server is now ready for client connections"))
                        .Build();

        await MsSqlContainer.StartAsync();
        
        ConnectionString = MsSqlContainer.GetConnectionString(DatabaseName);
    }

    public new virtual async Task DisposeAsync()
    {
        await MsSqlContainer.StopAsync();
    }
}

public class MsSqlWebApiTestFactory<TEndpoint, TDbContext> : WebApplicationFactory<TEndpoint>, IAsyncLifetime
    where TEndpoint : class
    where TDbContext : DbContext
{
    protected MsSqlContainer MsSqlContainer;

    protected readonly string DatabaseName = $"WebApi_IntegrationTest_{DateTime.Now:yyyyMMddHHmm}";

    protected string ConnectionString;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(serviceCollection =>
        {
            ReplaceDbContextOption<TDbContext>(serviceCollection);

            ConfigureTestServices(serviceCollection);
        });
    }

    protected virtual void ConfigureTestServices(IServiceCollection serviceCollection)
    {
    }

    protected virtual void ReplaceDbContextOption<TDbContextType>(IServiceCollection serviceCollection)
        where TDbContextType : DbContext
    {
        serviceCollection.Remove(serviceCollection.Single(a => typeof(DbContextOptions<TDbContextType>) ==
                                                               a.ServiceType));
        serviceCollection.AddDbContext<TDbContextType>(options =>
        {
            options.UseSqlServer(ConnectionString, c=>
            {
                c.EnableRetryOnFailure(3);
            });
        });
    }

    public virtual async Task InitializeAsync()
    {
        var currentTestingType = GetType().Name;

        var containerName = String.Concat("sqlserver-2022-", currentTestingType, $"-{Guid.NewGuid()}");

        MsSqlContainer = new MsSqlBuilder()
                        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
                        .WithPassword("Str0ng3stP@s5w0rd3ver!")
                        .WithName(containerName)
                        .WithWaitStrategy(Wait.ForUnixContainer()
                                              .UntilMessageIsLogged("SQL Server is now ready for client connections"))
                        .Build();

        await MsSqlContainer.StartAsync();

        ConnectionString = MsSqlContainer.GetConnectionString(DatabaseName);
    }

    public new virtual async Task DisposeAsync()
    {
        await MsSqlContainer.StopAsync();
    }
}