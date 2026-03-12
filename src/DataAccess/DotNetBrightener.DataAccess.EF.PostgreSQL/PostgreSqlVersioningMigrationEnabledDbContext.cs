using DotNetBrightener.DataAccess.Attributes;
using DotNetBrightener.DataAccess.EF.Migrations;
using DotNetBrightener.DataAccess.EF.PostgreSQL.Extensions;
using DotNetBrightener.DataAccess.EF.PostgreSQL.History;
using EntityFramework.Exceptions.PostgreSQL;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DotNetBrightener.DataAccess.EF.PostgreSQL;

/// <summary>
///    Represents the <see cref="DbContext" /> that defines the entities, has migrations applied and versioning enabled
/// </summary>
public abstract class PostgreSqlVersioningMigrationEnabledDbContext
    : AdvancedDbContext
{
    /// <summary>
    ///    Represents the <see cref="DbContext" /> that defines the entities, has migrations applied and versioning enabled
    /// </summary>
    protected PostgreSqlVersioningMigrationEnabledDbContext(DbContextOptions options)
        : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        optionsBuilder.UseExceptionProcessor();

        // Add PostgreSQL history interceptor to automatically create triggers
        optionsBuilder.AddPostgreSqlHistoryInterceptor(ServiceProvider);
    }

    protected sealed override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigureModelBuilder(modelBuilder);

        ConfigureHistoryTables(modelBuilder);

        modelBuilder.ApplyUuidV7DefaultForGuidKeys();
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
