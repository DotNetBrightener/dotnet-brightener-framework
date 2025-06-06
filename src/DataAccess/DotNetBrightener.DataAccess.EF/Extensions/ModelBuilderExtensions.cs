using DotNetBrightener.DataAccess.Attributes;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

// ReSharper disable CheckNamespace
namespace Microsoft.EntityFrameworkCore;

public static class ModelBuilderExtensions
{
    /// <summary>
    ///     Extends the <see cref="ModelBuilder.Entity{T}" /> method to register an entity with a temporal table,
    ///     if the entity type is defined with <see cref="HistoryEnabledAttribute"/> attribute.
    /// </summary>
    /// <typeparam name="T">
    ///     The entity type to register to the model if not a part of the model
    /// </typeparam>
    /// <param name="modelBuilder">
    ///     The <see cref="ModelBuilder"/>
    /// </param>
    /// <param name="tableName">
    ///     Specifies the name of the table
    /// </param>
    /// <param name="tableSchema">
    ///     Specifies the schema of the table
    /// </param>
    /// <returns></returns>
    public static EntityTypeBuilder<T> RegisterEntity<T>(this ModelBuilder modelBuilder,
                                                         string?           tableName   = null,
                                                         string?           tableSchema = null)
        where T : class
    {
        if (string.IsNullOrWhiteSpace(tableName))
        {
            tableName = typeof(T).Name;
        }

        if (typeof(T).HasAttribute<TableAttribute>(out var tableAttr))
        {
            tableName   = tableAttr.Name;
            tableSchema = tableAttr.Schema;
        }

        var entity = modelBuilder.Entity<T>();

        if (!typeof(T).HasAttribute<HistoryEnabledAttribute>(out var historyEnabledAttribute))
        {
            return entity.ToTable(tableName, schema: tableSchema);
        }

        entity.ToTable(tableName,
                       schema: tableSchema,
                       table => table.IsTemporal(t =>
                       {
                           t.UseHistoryTable($"{tableName}_History");
                       })
                      );

        return entity;
    }

    /// <summary>
    ///     Applies the configurations from all the <see cref="IEntityTypeConfiguration{TEntity}"/>
    ///     found in the assembly that invokes this method.
    /// </summary>
    /// <param name="modelBuilder">
    ///     The <see cref="ModelBuilder"/>
    /// </param>
    public static void ApplyConfigurationsFromCurrentAssembly(this ModelBuilder modelBuilder)
    {
        var derivedDbContextAssembly = Assembly.GetCallingAssembly();

        modelBuilder.ApplyConfigurationsFromAssembly(derivedDbContextAssembly);
    }
}