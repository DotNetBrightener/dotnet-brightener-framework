#nullable enable
using DotNetBrightener.DataAccess.EF.PostgreSQL.History;
using DotNetBrightener.DataAccess.EF.PostgreSQL.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

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
        services.AddScoped<PostgreSqlHistoryTableManager>();
        services.AddScoped<PostgreSqlHistoryInterceptor>();
        
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

        services.AddDbContext<TContext>((serviceProvider, options) =>
        {
            options.UseNpgsql(connectionString);
            
            // Add history interceptor
            var logger = serviceProvider.GetService<ILogger<PostgreSqlHistoryInterceptor>>();
            var historyManager = serviceProvider.GetService<PostgreSqlHistoryTableManager>();
            
            if (historyManager != null)
            {
                var interceptor = new PostgreSqlHistoryInterceptor(
                    logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<PostgreSqlHistoryInterceptor>.Instance,
                    historyManager);
                
                options.AddInterceptors(interceptor);
            }

            configureOptions?.Invoke(options);
        });

        services.TryAddScoped<PostgreSqlHistoryInterceptor>();
        services.AddDbContextConfigurator<PostgreSQlHistoryEnabledDbContextConfigurator>();

        return services;
    }
}
