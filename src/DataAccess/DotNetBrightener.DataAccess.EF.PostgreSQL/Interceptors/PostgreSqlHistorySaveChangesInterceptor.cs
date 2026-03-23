#nullable enable
using System.Data.Common;
using DotNetBrightener.DataAccess.Attributes;
using DotNetBrightener.DataAccess.EF.PostgreSQL.History;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace DotNetBrightener.DataAccess.EF.PostgreSQL.Interceptors;

/// <summary>
/// 	Interceptor that ensures history triggers are created before saving changes for history-enabled entities
/// </summary>
internal class PostgreSqlHistorySaveChangesInterceptor(
	ILogger<PostgreSqlHistorySaveChangesInterceptor> logger,
	PostgreSqlHistoryTableManager                   historyTableManager)
	: SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        if (eventData.Context is not null)
        {
            EnsureHistoryInfrastructureExists(eventData.Context);
        }

        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result,
                                                                                CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            EnsureHistoryInfrastructureExists(eventData.Context);
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void EnsureHistoryInfrastructureExists(DbContext context)
    {
        try
        {
            var connection = context.Database.GetDbConnection();

            // Only open if not already open
            if (connection.State == System.Data.ConnectionState.Closed)
            {
                connection.Open();
            }

            var model = context.Model;
            var historyEnabledEntities = model.GetEntityTypes()
                                              .Where(e => e.ClrType
                                                           .GetCustomAttributes(typeof(HistoryEnabledAttribute), true)
                                                           .Any())
                                              .ToList();

            if (!historyEnabledEntities.Any())
            {
                return;
            }

            foreach (var entityType in historyEnabledEntities)
            {
                CreateHistoryInfrastructure(connection, entityType);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to ensure history infrastructure before save");
        }
    }

    private void CreateHistoryInfrastructure(DbConnection connection, Microsoft.EntityFrameworkCore.Metadata.IEntityType entityType)
    {
        try
        {
            var tableName = entityType.GetTableName();
            var schema    = entityType.GetSchema();
            var triggerName = $"{tableName}_history_trigger";

            // 1. Create the history table
            var tableCreationSql = historyTableManager.GenerateHistoryTableSql(entityType);
            using var tableCommand = connection.CreateCommand();
            tableCommand.CommandText = tableCreationSql;
            tableCommand.ExecuteNonQuery();

            // 2. Create the trigger function (CREATE OR REPLACE is idempotent)
            var functionSql = historyTableManager.GenerateHistoryTriggerFunctionSql(entityType);
            using var functionCommand = connection.CreateCommand();
            functionCommand.CommandText = functionSql;
            functionCommand.ExecuteNonQuery();

            // 3. Create the trigger using conditional SQL
            var schemaCondition = string.IsNullOrEmpty(schema) ? "" : $"AND table_schema = '{schema}'";
            var qualifiedTable = string.IsNullOrEmpty(schema) ? $"\"{tableName}\"" : $"\"{schema}\".\"{tableName}\"";

            var triggerSql = $@"
DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = '{tableName}' {schemaCondition}) THEN
        IF NOT EXISTS (SELECT 1 FROM information_schema.triggers WHERE trigger_name = '{triggerName}' AND event_object_table = '{tableName}') THEN
            BEGIN
                EXECUTE 'DROP TRIGGER IF EXISTS {triggerName} ON {qualifiedTable}';
                EXECUTE 'CREATE TRIGGER {triggerName} BEFORE UPDATE OR DELETE ON {qualifiedTable} FOR EACH ROW EXECUTE FUNCTION {tableName}_history_trigger_func()';
            END;
        END IF;
    END IF;
END $$;";

            using var triggerCommand = connection.CreateCommand();
            triggerCommand.CommandText = triggerSql;
            triggerCommand.ExecuteNonQuery();

            logger.LogDebug("History infrastructure ensured for {TableName}", tableName);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to ensure history infrastructure for {EntityName}", entityType.ClrType.Name);
        }
    }
}
