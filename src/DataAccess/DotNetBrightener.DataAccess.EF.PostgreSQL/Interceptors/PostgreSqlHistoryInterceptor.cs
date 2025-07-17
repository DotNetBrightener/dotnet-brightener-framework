#nullable enable
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
    private readonly HashSet<string>                       _processedContexts   = new();

    public override async ValueTask<InterceptionResult> ConnectionOpeningAsync(
        DbConnection connection,
        ConnectionEventData eventData,
        InterceptionResult result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context != null)
        {
            await EnsureHistoryTriggersExist(eventData.Context, connection, cancellationToken);
        }

        return await base.ConnectionOpeningAsync(connection, eventData, result, cancellationToken);
    }

    public override InterceptionResult ConnectionOpening(
        DbConnection connection,
        ConnectionEventData eventData,
        InterceptionResult result)
    {
        if (eventData.Context != null)
        {
            EnsureHistoryTriggersExist(eventData.Context, connection, CancellationToken.None).GetAwaiter().GetResult();
        }

        return base.ConnectionOpening(connection, eventData, result);
    }

    private async Task EnsureHistoryTriggersExist(
        DbContext context,
        DbConnection connection,
        CancellationToken cancellationToken)
    {
        var contextKey = context.GetType().FullName ?? context.GetType().Name;
        
        // Avoid processing the same context type multiple times
        if (_processedContexts.Contains(contextKey))
            return;

        try
        {
            var model = context.Model;
            var historyEnabledEntities = model.GetEntityTypes()
                .Where(e => e.ClrType.GetCustomAttributes(typeof(HistoryEnabledAttribute), true).Any())
                .ToList();

            if (!historyEnabledEntities.Any())
            {
                _processedContexts.Add(contextKey);
                return;
            }

            logger.LogDebug("Creating history triggers for {Count} entities in context {ContextType}",
                historyEnabledEntities.Count, contextKey);

            foreach (var entityType in historyEnabledEntities)
            {
                await CreateHistoryTriggerIfNotExists(connection, entityType, cancellationToken);
            }

            _processedContexts.Add(contextKey);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to create history triggers for context {ContextType}", contextKey);
            // Don't throw - let the application continue without history triggers
        }
    }

    private async Task CreateHistoryTriggerIfNotExists(
        DbConnection connection,
        Microsoft.EntityFrameworkCore.Metadata.IEntityType entityType,
        CancellationToken cancellationToken)
    {
        try
        {
            var tableName = entityType.GetTableName();
            var schema = entityType.GetSchema();
            var triggerName = $"{tableName}_history_trigger";
            var functionName = $"{tableName}_history_function";

            // Check if trigger already exists
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
                    triggerName, tableName);

                var triggerSql = historyTableManager.GenerateHistoryTriggerSql(entityType);

                using var createCommand = connection.CreateCommand();
                createCommand.CommandText = triggerSql;
                await createCommand.ExecuteNonQueryAsync(cancellationToken);

                logger.LogInformation("Successfully created history trigger {TriggerName} for table {TableName}",
                    triggerName, tableName);
            }
            else
            {
                logger.LogDebug("History trigger {TriggerName} already exists for table {TableName}",
                    triggerName, tableName);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to create history trigger for entity {EntityName}",
                entityType.ClrType.Name);
            // Continue with other entities
        }
    }
}
