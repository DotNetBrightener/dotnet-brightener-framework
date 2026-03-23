#nullable enable
using System.Collections.Concurrent;
using System.Data.Common;
using DotNetBrightener.DataAccess.Attributes;
using DotNetBrightener.DataAccess.EF.PostgreSQL.History;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace DotNetBrightener.DataAccess.EF.PostgreSQL.Interceptors;

/// <summary>
/// 	Interceptor that ensures history triggers are created for PostgreSQL history-enabled entities
/// </summary>
internal class PostgreSqlHistoryInterceptor(
    ILogger<PostgreSqlHistoryInterceptor> logger,
    PostgreSqlHistoryTableManager         historyTableManager)
    : DbConnectionInterceptor
{

    public override async Task ConnectionOpenedAsync(DbConnection              connection,
                                                      ConnectionEndEventData eventData,
                                                      CancellationToken cancellationToken =
                                                          default)
    {
        if (eventData.Context != null)
        {
            await EnsureHistoryInfrastructureExists(eventData.Context, connection, cancellationToken);
        }

        await base.ConnectionOpenedAsync(connection, eventData, cancellationToken);
    }

    public override void ConnectionOpened(DbConnection          connection,
                                          ConnectionEndEventData eventData)
    {
        if (eventData.Context != null)
        {
            EnsureHistoryInfrastructureExistsSync(eventData.Context, connection);
        }

        base.ConnectionOpened(connection, eventData);
    }

    private async Task EnsureHistoryInfrastructureExists(DbContext         context,
                                                         DbConnection      connection,
                                                         CancellationToken cancellationToken)
    {
        // Don't cache - check every time in case tables were created after connection opened
        try
        {
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

            logger.LogDebug("Ensuring history infrastructure for {Count} entities", historyEnabledEntities.Count);

            foreach (var entityType in historyEnabledEntities)
            {
                await CreateHistoryInfrastructure(connection, entityType, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to create history infrastructure for context");
        }
    }

    private async Task CreateHistoryInfrastructure(DbConnection connection,
                                                    Microsoft.EntityFrameworkCore.Metadata.IEntityType entityType,
                                                    CancellationToken cancellationToken)
    {
        try
        {
            var tableName   = entityType.GetTableName();
            var schema      = entityType.GetSchema();
            var triggerName = $"{tableName}_history_trigger";

            // 1. Create the history table
            logger.LogDebug("Ensuring history table exists for {TableName}", tableName);
            var tableCreationSql = historyTableManager.GenerateHistoryTableSql(entityType);

            using var tableCommand = connection.CreateCommand();
            tableCommand.CommandText = tableCreationSql;
            await tableCommand.ExecuteNonQueryAsync(cancellationToken);

            // 2. Create the trigger function (CREATE OR REPLACE is idempotent)
            logger.LogDebug("Ensuring history trigger function exists for {TableName}", tableName);
            var functionSql = historyTableManager.GenerateHistoryTriggerFunctionSql(entityType);

            using var functionCommand = connection.CreateCommand();
            functionCommand.CommandText = functionSql;
            await functionCommand.ExecuteNonQueryAsync(cancellationToken);

            // 3. Create the trigger using conditional SQL - run every time to ensure it exists
            var schemaCondition = string.IsNullOrEmpty(schema) ? "" : $"AND table_schema = '{schema}'";
            var qualifiedTable = string.IsNullOrEmpty(schema) ? $"\"{tableName}\"" : $"\"{schema}\".\"{tableName}\"";

            var triggerSql = $@"
DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = '{tableName}' {schemaCondition}) THEN
        IF NOT EXISTS (SELECT 1 FROM information_schema.triggers WHERE trigger_name = '{triggerName}' AND event_object_table = '{tableName}') THEN
            EXECUTE 'DROP TRIGGER IF EXISTS {triggerName} ON {qualifiedTable}';
            EXECUTE 'CREATE TRIGGER {triggerName} BEFORE UPDATE OR DELETE ON {qualifiedTable} FOR EACH ROW EXECUTE FUNCTION {tableName}_history_trigger_func()';
        END IF;
    END IF;
END $$;";

            using var triggerCommand = connection.CreateCommand();
            triggerCommand.CommandText = triggerSql;
            await triggerCommand.ExecuteNonQueryAsync(cancellationToken);

            logger.LogDebug("History infrastructure ensured for {TableName}", tableName);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to ensure history infrastructure for {EntityName}", entityType.ClrType.Name);
        }
    }

    private void EnsureHistoryInfrastructureExistsSync(DbContext context, DbConnection connection)
    {
        // Don't cache - check every time in case tables were created after connection opened
        try
        {
            var historyEnabledEntities = context.Model
                                                .GetEntityTypes()
                                                .Where(e => e.ClrType
                                                             .GetCustomAttributes(typeof(HistoryEnabledAttribute), true)
                                                             .Any())
                                                .ToList();

            if (!historyEnabledEntities.Any())
            {
                return;
            }

            logger.LogDebug("Ensuring history infrastructure (sync) for {Count} entities", historyEnabledEntities.Count);

            foreach (var entityType in historyEnabledEntities)
            {
                CreateHistoryInfrastructureSync(connection, entityType);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to create history infrastructure (sync) for context");
        }
    }

    private void CreateHistoryInfrastructureSync(
        DbConnection                                      connection,
        Microsoft.EntityFrameworkCore.Metadata.IEntityType entityType)
    {
        try
        {
            var tableName   = entityType.GetTableName();
            var schema      = entityType.GetSchema();
            var triggerName = $"{tableName}_history_trigger";

            // 1. Create the history table
            using var tableCommand = connection.CreateCommand();
            tableCommand.CommandText = historyTableManager.GenerateHistoryTableSql(entityType);
            tableCommand.ExecuteNonQuery();

            // 2. Create the trigger function (CREATE OR REPLACE is idempotent)
            logger.LogDebug("Ensuring history trigger function exists (sync) for {TableName}", tableName);
            using var functionCommand = connection.CreateCommand();
            functionCommand.CommandText = historyTableManager.GenerateHistoryTriggerFunctionSql(entityType);
            functionCommand.ExecuteNonQuery();

            // 3. Create the trigger using conditional SQL
            var schemaCondition = string.IsNullOrEmpty(schema) ? "" : $"AND table_schema = '{schema}'";
            var qualifiedTable = string.IsNullOrEmpty(schema) ? $"\"{tableName}\"" : $"\"{schema}\".\"{tableName}\"";

            var triggerSql = $@"
DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = '{tableName}' {schemaCondition}) THEN
        IF NOT EXISTS (SELECT 1 FROM information_schema.triggers WHERE trigger_name = '{triggerName}' AND event_object_table = '{tableName}') THEN
            EXECUTE 'DROP TRIGGER IF EXISTS {triggerName} ON {qualifiedTable}';
            EXECUTE 'CREATE TRIGGER {triggerName} BEFORE UPDATE OR DELETE ON {qualifiedTable} FOR EACH ROW EXECUTE FUNCTION {tableName}_history_trigger_func()';
        END IF;
    END IF;
END $$;";

            using var triggerCommand = connection.CreateCommand();
            triggerCommand.CommandText = triggerSql;
            triggerCommand.ExecuteNonQuery();

            logger.LogDebug("History infrastructure ensured (sync) for {TableName}", tableName);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to ensure history infrastructure (sync) for {EntityName}", entityType.ClrType.Name);
        }
    }
}
