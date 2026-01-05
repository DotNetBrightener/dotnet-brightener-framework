using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MsSql;
using Xunit;

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
        try
        {
            var currentTestingType = GetType().Name;

            var containerName = String.Concat("sqlserver-2022-", currentTestingType, $"-{Guid.NewGuid()}");

            MsSqlContainer = MsSqlContainerGenerator.CreateContainer(containerName);

            await MsSqlContainer.StartAsync();

            ConnectionString = MsSqlContainer.GetConnectionStringForDb(DatabaseName);
        }
        catch (Exception)
        {
            if (MsSqlContainer is not null)
            {
                await MsSqlContainer.StopAsync();
            }
            throw;
        }
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
            options.UseSqlServer(ConnectionString,
                                 c =>
                                 {
                                     c.EnableRetryOnFailure(3);
                                 });
        });
    }

    public virtual async Task InitializeAsync()
    {
        try
        {
            var currentTestingType = GetType().Name;

            var containerName = String.Concat("sqlserver-2022-", currentTestingType, $"-{Guid.NewGuid()}");

            MsSqlContainer = MsSqlContainerGenerator.CreateContainer(containerName);

            await MsSqlContainer.StartAsync();

            ConnectionString = MsSqlContainer.GetConnectionStringForDb(DatabaseName);
        }
        catch (Exception)
        {
            if (MsSqlContainer is not null)
            {
                await MsSqlContainer.StopAsync();
            }

            throw;
        }
    }

    public new virtual async Task DisposeAsync()
    {
        await MsSqlContainer.StopAsync();
    }
}