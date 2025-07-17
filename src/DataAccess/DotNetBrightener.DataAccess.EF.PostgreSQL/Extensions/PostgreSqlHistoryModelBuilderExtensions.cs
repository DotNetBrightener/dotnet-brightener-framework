#nullable enable
using DotNetBrightener.DataAccess.EF.Migrations;
using DotNetBrightener.DataAccess.EF.PostgreSQL.History;
using DotNetBrightener.DataAccess.EF.PostgreSQL.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DotNetBrightener.DataAccess.EF.PostgreSQL.Extensions;

/// <summary>
/// Extension methods for configuring PostgreSQL history tables
/// </summary>
public static class PostgreSqlHistoryModelBuilderExtensions
{
    /// <summary>
    /// Configures PostgreSQL history tables for entities marked with HistoryEnabledAttribute
    /// </summary>
    /// <param name="modelBuilder">The model builder</param>
    /// <param name="logger">Optional logger for debugging</param>
    /// <returns>The model builder for chaining</returns>
    public static ModelBuilder ConfigurePostgreSqlHistoryTables(
        this ModelBuilder modelBuilder,
        ILogger? logger = null)
    {
        var historyManager = new PostgreSqlHistoryTableManager(
            logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<PostgreSqlHistoryTableManager>.Instance);

        historyManager.ConfigureHistoryTables(modelBuilder);

        return modelBuilder;
    }

    /// <summary>
    /// Adds PostgreSQL history interceptor to the DbContext options
    /// </summary>
    /// <param name="optionsBuilder">The options builder</param>
    /// <param name="logger">Optional logger</param>
    /// <returns>The options builder for chaining</returns>
    public static DbContextOptionsBuilder AddPostgreSqlHistoryInterceptor(
        this DbContextOptionsBuilder optionsBuilder,
        IServiceProvider serviceProvider)
    {
        var historyTableManager = new PostgreSqlHistoryTableManager(serviceProvider);

        var interceptor = new Interceptors.PostgreSqlHistoryInterceptor(interceptorLogger, historyTableManager);

        optionsBuilder.AddInterceptors(interceptor);

        return optionsBuilder;
    }
}





internal class PostgreSQlHistoryEnabledDbContextConfigurator(
    PostgreSqlHistoryInterceptor postgreSqlHistoryInterceptor) : IDbContextConfigurator
{
    public void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(postgreSqlHistoryInterceptor);
    }
}
