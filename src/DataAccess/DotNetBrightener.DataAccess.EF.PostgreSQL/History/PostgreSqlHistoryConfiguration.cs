#nullable enable
using DotNetBrightener.DataAccess.Attributes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace DotNetBrightener.DataAccess.EF.PostgreSQL.History;

/// <summary>
/// Configuration settings for PostgreSQL history tables
/// </summary>
internal static class PostgreSqlHistoryConfiguration
{
    /// <summary>
    /// Gets the history table name for an entity
    /// </summary>
    public static string GetHistoryTableName(IEntityType entityType)
    {
        var historyTableName = entityType.GetAnnotation("PostgreSQL:HistoryTableName")?.Value as string;
        return historyTableName ?? $"{entityType.GetTableName()}_History";
    }

    /// <summary>
    /// Gets the history table schema for an entity
    /// </summary>
    public static string? GetHistoryTableSchema(IEntityType entityType)
    {
        return entityType.GetAnnotation("PostgreSQL:HistoryTableSchema")?.Value as string 
               ?? entityType.GetSchema();
    }

    /// <summary>
    /// Checks if an entity has history tracking enabled
    /// </summary>
    public static bool IsHistoryEnabled(IEntityType entityType)
    {
        return entityType.ClrType.GetCustomAttributes(typeof(HistoryEnabledAttribute), true).Any();
    }

    /// <summary>
    /// Gets the full qualified history table name (schema.table)
    /// </summary>
    public static string GetFullHistoryTableName(IEntityType entityType)
    {
        var tableName = GetHistoryTableName(entityType);
        var schema = GetHistoryTableSchema(entityType);
        
        return string.IsNullOrEmpty(schema) ? tableName : $"{schema}.{tableName}";
    }

    /// <summary>
    /// Gets the full qualified main table name (schema.table)
    /// </summary>
    public static string GetFullMainTableName(IEntityType entityType)
    {
        var tableName = entityType.GetTableName();
        var schema = entityType.GetSchema();
        
        return string.IsNullOrEmpty(schema) ? tableName : $"{schema}.{tableName}";
    }
}
