#nullable enable
using DotNetBrightener.DataAccess.Attributes;
using DotNetBrightener.DataAccess.EF.PostgreSQL.History;
using DotNetBrightener.DataAccess.EF.Repositories;
using DotNetBrightener.DataAccess.Exceptions;
using Microsoft.EntityFrameworkCore;
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
        Microsoft.EntityFrameworkCore.Metadata.IEntityType entityType,
        string historyTableName,
        string? historyTableSchema,
        string? mainTableName,
        string? mainTableSchema,
        DateTimeOffset? from,
        DateTimeOffset? to) where T : class, new()
    {
        var fullHistoryTableName = string.IsNullOrEmpty(historyTableSchema)
            ? historyTableName
            : $"{historyTableSchema}.{historyTableName}";

        var fullMainTableName = string.IsNullOrEmpty(mainTableSchema)
            ? mainTableName
            : $"{mainTableSchema}.{mainTableName}";

        // Get all columns except the period columns
        var columns = entityType.GetProperties()
            .Where(p => !p.IsShadowProperty())
            .Select(p => p.Name)
            .ToList();

        var columnList = string.Join(", ", columns.Select(c => $"\"{c}\""));

        // For current data, we need to add dummy period columns to match history table structure
        var currentDataSql = $@"
            SELECT {columnList},
                   COALESCE(""ModifiedDate"", ""CreatedDate"", NOW()) as ""PeriodStart"",
                   '9999-12-31 23:59:59.999999+00'::timestamptz as ""PeriodEnd""
            FROM {fullMainTableName}";

        var historyDataSql = $@"
            SELECT {columnList}, ""PeriodStart"", ""PeriodEnd""
            FROM {fullHistoryTableName}";

        // Add date range filtering if specified
        if (from.HasValue || to.HasValue)
        {
            var whereConditions = new List<string>();

            if (from.HasValue)
            {
                whereConditions.Add($"\"PeriodStart\" >= '{from.Value:yyyy-MM-dd HH:mm:ss.fff}+00'");
            }

            if (to.HasValue)
            {
                whereConditions.Add($"\"PeriodEnd\" <= '{to.Value:yyyy-MM-dd HH:mm:ss.fff}+00'");
            }

            if (whereConditions.Any())
            {
                var whereClause = string.Join(" AND ", whereConditions);
                historyDataSql += $" WHERE {whereClause}";

                // Also filter current data if it falls within the date range
                currentDataSql += $" WHERE {whereClause.Replace("\"PeriodEnd\"", "'9999-12-31 23:59:59.999999+00'::timestamptz")}";
            }
        }

        // Combine current and historical data, then order by PeriodStart
        var sql = $@"
            SELECT {columnList} FROM (
                {currentDataSql}
                UNION ALL
                {historyDataSql}
            ) combined_data
            ORDER BY ""PeriodStart"" DESC";

        return sql;
    }
}
