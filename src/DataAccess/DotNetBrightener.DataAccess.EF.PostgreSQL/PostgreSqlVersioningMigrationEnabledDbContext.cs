using DotNetBrightener.DataAccess.Attributes;
using DotNetBrightener.DataAccess.EF.Migrations;
using DotNetBrightener.DataAccess.EF.PostgreSQL.Extensions;
using DotNetBrightener.DataAccess.EF.PostgreSQL.History;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DotNetBrightener.DataAccess.EF.PostgreSQL;

/// <summary>
///    Represents the <see cref="DbContext" /> that defines the entities, has migrations applied and versioning enabled
/// </summary>
public abstract class PostgreSqlVersioningMigrationEnabledDbContext(
    DbContextOptions options)
    : AdvancedDbContext(options)
{
    protected sealed override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigureModelBuilder(modelBuilder);

        ConfigureHistoryTables(modelBuilder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        // Add PostgreSQL history interceptor to automatically create triggers
        optionsBuilder.AddPostgreSqlHistoryInterceptor(ServiceProvider);
    }

    /// <summary>
    ///     Registers the entities to the <see cref="ModelBuilder"/>
    /// </summary>
    /// <param name="modelBuilder">
    ///     The <see cref="ModelBuilder"/> to register the entity types for the DbContext
    /// </param>
    protected abstract void ConfigureModelBuilder(ModelBuilder modelBuilder);

    protected virtual void ConfigureHistoryTables(ModelBuilder modelBuilder)
    {
        var historyTableManager = new PostgreSqlHistoryTableManager(ServiceProvider);

        historyTableManager.ConfigureHistoryTables(modelBuilder);
    }
}
