#nullable enable
using DotNetBrightener.DataAccess.EF.PostgreSQL;
using DotNetBrightener.DataAccess.EF.PostgreSQL.History;
using DotNetBrightener.DataAccess.EF.PostgreSQL.Interceptors;
using DotNetBrightener.DataAccess.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DotNetBrightener.DataAccess.EF.PostgreSQL.Extensions;

/// <summary>
/// Extension methods for configuring PostgreSQL history services
/// </summary>
public static class PostgreSqlHistoryServiceCollectionExtensions
{
    /// <summary>
    /// Adds PostgreSQL history services to the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddPostgreSqlHistoryServices(this IServiceCollection services)
    {
        services.TryAddSingleton<PostgreSqlHistoryTableManager>();
        services.TryAddSingleton<PostgreSqlHistoryInterceptor>();

        services.Replace(ServiceDescriptor.Scoped<IRepository, PostgreSqlRepository>());

        return services;
    }

    /// <summary>
    /// Configures a DbContext to use PostgreSQL history tracking
    /// </summary>
    /// <typeparam name="TContext">The DbContext type</typeparam>
    /// <param name="services">The service collection</param>
    /// <param name="connectionString">The PostgreSQL connection string</param>
    /// <param name="configureOptions">Optional additional configuration</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddPostgreSqlDbContextWithHistory<TContext>(
        this IServiceCollection services,
        string connectionString,
        Action<DbContextOptionsBuilder>? configureOptions = null)
        where TContext : DbContext
    {
        services.AddPostgreSqlHistoryServices();

        services.AddDbContext<TContext>((_, options) =>
        {
            options.UseNpgsql(connectionString);
            configureOptions?.Invoke(options);
        });

        // The interceptor is wired into every DbContext through the IDbContextConfigurator
        // pipeline (PostgreSQlHistoryEnabledDbContextConfigurator). No manual instantiation
        // needed here — that was the source of the duplicate-registration bug.
        services.AddDbContextConfigurator<PostgreSQlHistoryEnabledDbContextConfigurator>();

        return services;
    }
}
