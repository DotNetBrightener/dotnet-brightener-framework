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
/// Interceptor that ensures history triggers are created for PostgreSQL history-enabled entities
/// </summary>
internal class PostgreSqlHistoryInterceptor(
    ILogger<PostgreSqlHistoryInterceptor> logger,
    PostgreSqlHistoryTableManager         historyTableManager)
    : DbConnectionInterceptor
{
    // Static so the "already processed" cache survives across scoped lifetimes when the
    // interceptor is registered as Singleton — one check per context type per process lifetime.
    private static readonly ConcurrentDictionary<string, bool> _processedContexts = new();

    public override async ValueTask<InterceptionResult> ConnectionOpeningAsync(DbConnection        connection,
                                                                               ConnectionEventData eventData,
                                                                               InterceptionResult  result,
                                                                               CancellationToken cancellationToken =
                                                                                   default)
    {
        if (eventData.Context != null)
        {
            await EnsureHistoryTriggersExist(eventData.Context, connection, cancellationToken);
        }

        return await base.ConnectionOpeningAsync(connection, eventData, result, cancellationToken);
    }

    public override InterceptionResult ConnectionOpening(DbConnection        connection,
                                                         ConnectionEventData eventData,
                                                         InterceptionResult  result)
    {
        if (eventData.Context != null)
        {
            // Use the fully-synchronous code path to avoid .GetAwaiter().GetResult()
            // which can deadlock inside an ASP.NET synchronization context.
            EnsureHistoryTriggersExistSync(eventData.Context, connection);
        }

        return base.ConnectionOpening(connection, eventData, result);
    }

    private async Task EnsureHistoryTriggersExist(DbContext         context,
                                                  DbConnection      connection,
                                                  CancellationToken cancellationToken)
    {
        // Include connection string in cache key to support multiple databases (e.g., tests with separate containers)
        var contextKey = $"{context.GetType().FullName ?? context.GetType().Name}_{connection.ConnectionString}";

        // Avoid processing the same context type more than once per process lifetime.
        if (_processedContexts.ContainsKey(contextKey))
            return;

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
                _processedContexts.TryAdd(contextKey, true);

                return;
            }

            logger.LogDebug("Creating history triggers for {Count} entities in context {ContextType}",
                            historyEnabledEntities.Count,
                            contextKey);

            foreach (var entityType in historyEnabledEntities)
            {
                await CreateHistoryTriggerIfNotExists(connection, entityType, cancellationToken);
            }

            _processedContexts.TryAdd(contextKey, true);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to create history triggers for context {ContextType}", contextKey);
            // Don't throw - let the application continue without history triggers
        }
    }

    private async Task CreateHistoryTriggerIfNotExists(DbConnection connection,
                                                       Microsoft.EntityFrameworkCore.Metadata.IEntityType entityType,
                                                       CancellationToken cancellationToken)
    {
        try
        {
            var tableName   = entityType.GetTableName();
            var schema      = entityType.GetSchema();
            var triggerName = $"{tableName}_history_trigger";

            // Ensure the history table exists before we check / create the trigger.
            // CREATE TABLE IF NOT EXISTS is idempotent, so this is safe to run every time
            // the application starts (when the processed-context cache is cold).
            logger.LogDebug("Ensuring history table exists for {TableName}", tableName);
            var tableCreationSql = historyTableManager.GenerateHistoryTableSql(entityType);

            using var tableCommand = connection.CreateCommand();
            tableCommand.CommandText = tableCreationSql;
            await tableCommand.ExecuteNonQueryAsync(cancellationToken);

            // Check if the trigger already exists
            var checkTriggerSql = $@"
                SELECT COUNT(*)
                FROM information_schema.triggers
                WHERE trigger_name = '{triggerName}'
                AND event_object_table = '{tableName}'
                {(string.IsNullOrEmpty(schema) ? "" : $"AND event_object_schema = '{schema}'")}";

            using var checkCommand = connection.CreateCommand();
            checkCommand.CommandText = checkTriggerSql;

            var triggerExists = Convert.ToInt32(await checkCommand.ExecuteScalarAsync(cancellationToken)) > 0;

            if (!triggerExists)
            {
                logger.LogDebug("Creating history trigger {TriggerName} for table {TableName}",
                                triggerName,
                                tableName);

                var triggerSql = historyTableManager.GenerateHistoryTriggerSql(entityType);

                using var createCommand = connection.CreateCommand();
                createCommand.CommandText = triggerSql;
                await createCommand.ExecuteNonQueryAsync(cancellationToken);

                logger.LogInformation("Successfully created history trigger {TriggerName} for table {TableName}",
                                      triggerName,
                                      tableName);
            }
            else
            {
                logger.LogDebug("History trigger {TriggerName} already exists for table {TableName}",
                                triggerName,
                                tableName);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                            "Failed to ensure history infrastructure for entity {EntityName}",
                            entityType.ClrType.Name);
            // Continue with other entities
        }
    }

    // -----------------------------------------------------------------------
    // Synchronous counterparts — used by ConnectionOpening (the non-async
    // override) to avoid the sync-over-async deadlock that .GetAwaiter().GetResult()
    // can cause inside an ASP.NET synchronization context.
    // -----------------------------------------------------------------------

    private void EnsureHistoryTriggersExistSync(DbContext context, DbConnection connection)
    {
        // Include connection string in cache key to support multiple databases (e.g., tests with separate containers)
        var contextKey = $"{context.GetType().FullName ?? context.GetType().Name}_{connection.ConnectionString}";

        if (_processedContexts.ContainsKey(contextKey))
            return;

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
                _processedContexts.TryAdd(contextKey, true);
                return;
            }

            logger.LogDebug("Creating history triggers (sync) for {Count} entities in context {ContextType}",
                            historyEnabledEntities.Count,
                            contextKey);

            foreach (var entityType in historyEnabledEntities)
            {
                CreateHistoryInfrastructureSync(connection, entityType);
            }

            _processedContexts.TryAdd(contextKey, true);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to create history triggers (sync) for context {ContextType}", contextKey);
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

            // Ensure the history table exists (idempotent)
            using var tableCommand = connection.CreateCommand();
            tableCommand.CommandText = historyTableManager.GenerateHistoryTableSql(entityType);
            tableCommand.ExecuteNonQuery();

            // Check whether the trigger already exists
            var checkTriggerSql = $@"
                SELECT COUNT(*)
                FROM information_schema.triggers
                WHERE trigger_name = '{triggerName}'
                AND event_object_table = '{tableName}'
                {(string.IsNullOrEmpty(schema) ? "" : $"AND event_object_schema = '{schema}'")}";

            using var checkCommand = connection.CreateCommand();
            checkCommand.CommandText = checkTriggerSql;
            var triggerExists = Convert.ToInt32(checkCommand.ExecuteScalar()) > 0;

            if (!triggerExists)
            {
                logger.LogDebug("Creating history trigger {TriggerName} for table {TableName} (sync)",
                                triggerName, tableName);

                using var createCommand = connection.CreateCommand();
                createCommand.CommandText = historyTableManager.GenerateHistoryTriggerSql(entityType);
                createCommand.ExecuteNonQuery();

                logger.LogInformation("Successfully created history trigger {TriggerName} for table {TableName}",
                                      triggerName, tableName);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                            "Failed to ensure history infrastructure (sync) for entity {EntityName}",
                            entityType.ClrType.Name);
        }
    }
}