#nullable enable
using DotNetBrightener.DataAccess.Attributes;
using DotNetBrightener.DataAccess.EF.PostgreSQL.History;
using DotNetBrightener.DataAccess.EF.Repositories;
using DotNetBrightener.DataAccess.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace DotNetBrightener.DataAccess.EF.PostgreSQL;

public class PostgreSqlRepository(
    DbContext        dbContext,
    IServiceProvider serviceProvider,
    ILoggerFactory   loggerFactory)
    : Repository(dbContext, serviceProvider, loggerFactory)
{
    public override IQueryable<T> FetchHistory<T>(Expression<Func<T, bool>>? expression,
                                                  DateTimeOffset?            from,
                                                  DateTimeOffset?            to)
    {
        if (!typeof(T).HasAttribute<HistoryEnabledAttribute>())
        {
            throw new VersioningNotSupportedException<T>();
        }

        var entityType = DbContext.Model.FindEntityType(typeof(T));
        if (entityType == null)
        {
            throw new InvalidOperationException($"Entity type {typeof(T).Name} is not configured in the DbContext");
        }

        // Get history table information
        var historyTableName = PostgreSqlHistoryConfiguration.GetHistoryTableName(entityType);
        var historyTableSchema = PostgreSqlHistoryConfiguration.GetHistoryTableSchema(entityType);
        var mainTableName = entityType.GetTableName();
        var mainTableSchema = entityType.GetSchema();

        // Build the SQL query to fetch history data
        var sql = BuildHistoryQuery<T>(entityType, historyTableName, historyTableSchema,
                                      mainTableName, mainTableSchema, from, to);

        Logger.LogDebug("Executing PostgreSQL history query: {Sql}", sql);

        // Execute raw SQL query, use AsNoTracking so that multiple historical snapshots
        // with the same primary key are not collapsed by EF's identity map.
        // Then apply the caller's LINQ filter on top of the raw result set.
        var query = DbContext.Set<T>().FromSqlRaw(sql).AsNoTracking();

        return expression is not null ? query.Where(expression) : query;
    }

    private string BuildHistoryQuery<T>(
        IEntityType entityType,
        string historyTableName,
        string? historyTableSchema,
        string? mainTableName,
        string? mainTableSchema,
        DateTimeOffset? from,
        DateTimeOffset? to) where T : class, new()
    {
        var fullHistoryTableName = string.IsNullOrEmpty(historyTableSchema)
            ? $"\"{historyTableName}\""
            : $"\"{historyTableSchema}\".\"{historyTableName}\"";

        var fullMainTableName = string.IsNullOrEmpty(mainTableSchema)
            ? $"\"{mainTableName}\""
            : $"\"{mainTableSchema}\".\"{mainTableName}\"";

        // Get all columns except the period columns.
        // Use the actual DB column name so that [Column] attributes and naming conventions
        // (e.g. Npgsql snake_case) are correctly reflected in the generated SQL.
        var storeObject = StoreObjectIdentifier.Table(mainTableName!, mainTableSchema);
        var columns = entityType.GetProperties()
            .Where(p => !p.IsShadowProperty())
            .Select(p => p.GetColumnName(storeObject) ?? p.Name)
            .ToList();

        var columnList = string.Join(", ", columns.Select(c => $"\"{c}\""));

        var currentDataSql = $@"
            SELECT {columnList}
            FROM {fullMainTableName}";

        var historyDataSql = $@"
            SELECT {columnList}
            FROM {fullHistoryTableName}";

        // Add date range filtering if specified
        if (from.HasValue || to.HasValue)
        {
            var historyWhereConditions = new List<string>();

            // For history table, filter by PeriodStart/PeriodEnd overlap with the requested range
            // A record overlaps if: PeriodStart <= to AND PeriodEnd >= from
            if (to.HasValue)
            {
                historyWhereConditions.Add($"\"PeriodStart\" <= '{to.Value:yyyy-MM-dd HH:mm:ss.fff}+00'");
            }

            if (from.HasValue)
            {
                historyWhereConditions.Add($"\"PeriodEnd\" >= '{from.Value:yyyy-MM-dd HH:mm:ss.fff}+00'");
            }

            if (historyWhereConditions.Any())
            {
                var historyWhereClause = string.Join(" AND ", historyWhereConditions);
                historyDataSql += $" WHERE {historyWhereClause}";
            }

            // For current data, filter by ModifiedDate/CreatedDate being <= to
            // (current records are valid from their creation/modification until "forever")
            // We only need to check that the record was active before the 'to' date
            if (to.HasValue)
            {
                currentDataSql += $" WHERE COALESCE(\"ModifiedDate\", \"CreatedDate\", NOW()) <= '{to.Value:yyyy-MM-dd HH:mm:ss.fff}+00'";
            }

            if (from.HasValue)
            {
                // For current records with a 'from' filter, they're always valid since PeriodEnd is "forever"
                // Just ensure they existed before the 'from' time
                var existingCondition = $" COALESCE(\"ModifiedDate\", \"CreatedDate\", NOW()) <= '{from.Value:yyyy-MM-dd HH:mm:ss.fff}+00'";
                currentDataSql += currentDataSql.Contains("WHERE") ? $" AND {existingCondition}" : $" WHERE {existingCondition}";
            }
        }

        // Combine current and historical data
        var sql = $@"
            SELECT {columnList} FROM (
                {currentDataSql}
                UNION ALL
                {historyDataSql}
            ) combined_data";

        return sql;
    }
}
