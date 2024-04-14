﻿using DotNetBrightener.DataAccess.Attributes;
using DotNetBrightener.DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace DotNetBrightener.DataAccess.EF.Migrations;

/// <summary>
///    Represents the <see cref="DbContext" /> that defines the entities, has migrations applied and versioning enabled
/// </summary>
public abstract class SqlServerVersioningMigrationEnabledDbContext : MigrationEnabledDbContext
{
    protected SqlServerVersioningMigrationEnabledDbContext(DbContextOptions options)
        : base(options)
    {

    }

    protected sealed override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureModelBuilder(modelBuilder);

        ConfigureHistoryTables(modelBuilder);

        modelBuilder.Model
                    .GetEntityTypes()
                    .ToList()
                    .ForEach(entityType =>
        {
            if (entityType.ClrType.IsAssignableTo(typeof(BaseEntity<Guid>)))
            {
                modelBuilder.Entity(entityType.ClrType)
                            .Property(nameof(BaseEntity.Id))
                            .ValueGeneratedOnAdd()
                            .HasDefaultValueSql("NEWSEQUENTIALID()");
            }
        });
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
        // read models from modelBuilder
        var models = modelBuilder.Model.GetEntityTypes();

        // detect HistoryEnabled entities
        var historyEnabledEntities =
            models.Where(x => x.ClrType.GetCustomAttributes(typeof(HistoryEnabledAttribute), true).Any());

        foreach (var entityType in historyEnabledEntities)
        {
            entityType.SetIsTemporal(true);

            var tableName = entityType.GetTableName();

            entityType.SetHistoryTableName($"{tableName}_History");
        }
    }
}