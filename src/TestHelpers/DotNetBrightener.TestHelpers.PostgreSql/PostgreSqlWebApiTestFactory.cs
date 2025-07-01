using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using Xunit;

namespace DotNetBrightener.TestHelpers.PostgreSql;

public class PostgreSqlWebApiTestFactory<TEndpoint> : WebApplicationFactory<TEndpoint>, IAsyncLifetime
    where TEndpoint : class
{
    protected         PostgreSqlContainer PostgreSqlContainer;
    protected virtual string              DatabaseName => $"WebApi_IntegrationTest_{DateTime.Now:yyyyMMddHHmm}";
    protected virtual int?                ExposedPort  => null;

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
            options.UseNpgsql(ConnectionString,
                              c =>
                              {
                                  c.EnableRetryOnFailure(3);
                              });
        });
    }

    public virtual async Task InitializeAsync()
    {
        PostgreSqlContainer = PostgreSqlContainerGenerator.CreateContainer(DatabaseName, ExposedPort);

        await PostgreSqlContainer.StartAsync();

        ConnectionString = PostgreSqlContainer.GetConnectionString();
    }

    public new virtual async Task DisposeAsync()
    {
        await PostgreSqlContainer.StopAsync();
    }
}

public class PostgreSqlWebApiTestFactory<TEndpoint, TDbContext> : WebApplicationFactory<TEndpoint>, IAsyncLifetime
    where TEndpoint : class
    where TDbContext : DbContext
{
    protected PostgreSqlContainer PostgreSqlContainer;

    protected virtual string DatabaseName => $"WebApi_IntegrationTest_{DateTime.Now:yyyyMMddHHmm}";
    protected virtual int?   ExposedDatabasePort  => null;

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
            options.UseNpgsql(ConnectionString,
                              c =>
                              {
                                  c.EnableRetryOnFailure(3);
                              });
        });
    }

    public virtual async Task InitializeAsync()
    {
        PostgreSqlContainer = PostgreSqlContainerGenerator.CreateContainer(DatabaseName, ExposedDatabasePort);

        await PostgreSqlContainer.StartAsync();

        ConnectionString = PostgreSqlContainer.GetConnectionString();
    }

    public new virtual async Task DisposeAsync()
    {
        await PostgreSqlContainer.StopAsync();
    }
}