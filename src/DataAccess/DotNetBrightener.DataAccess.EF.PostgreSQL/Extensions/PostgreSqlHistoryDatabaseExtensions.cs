#nullable enable
using DotNetBrightener.DataAccess.Attributes;
using DotNetBrightener.DataAccess.EF.PostgreSQL.History;
using Microsoft.EntityFrameworkCore;

namespace DotNetBrightener.DataAccess.EF.PostgreSQL.Extensions;

/// <summary>
/// 	Extension methods for manually initializing PostgreSQL history infrastructure
/// </summary>
public static class PostgreSqlHistoryDatabaseExtensions
{
	/// <summary>
	/// 	Creates history infrastructure (tables, functions, and triggers) for all history-enabled entities.
	/// 	This should be called after Database.EnsureCreated() or Database.Migrate().
	/// </summary>
	/// <param name="context">The DbContext</param>
	/// <param name="serviceProvider">The service provider</param>
	/// <param name="cancellationToken">Cancellation token</param>
	public static async Task CreateHistoryInfrastructureAsync(
		this DbContext context,
		IServiceProvider serviceProvider,
		CancellationToken cancellationToken = default)
	{
		var historyTableManager = serviceProvider.GetService(typeof(PostgreSqlHistoryTableManager)) as PostgreSqlHistoryTableManager;
		if (historyTableManager == null)
		{
			throw new InvalidOperationException("PostgreSqlHistoryTableManager is not registered in the service provider. Did you call AddPostgreSqlHistoryServices()?");
		}

		var connection = context.Database.GetDbConnection();
		var shouldClose = false;

		if (connection.State == System.Data.ConnectionState.Closed)
		{
			await connection.OpenAsync(cancellationToken);
			shouldClose = true;
		}

		try
		{
			var historyEnabledEntities = context.Model.GetEntityTypes()
													  .Where(e => e.ClrType
																		   .GetCustomAttributes(typeof(HistoryEnabledAttribute), true)
																		   .Any())
													  .ToList();

			foreach (var entityType in historyEnabledEntities)
			{
				await CreateHistoryInfrastructureAsync(connection, entityType, historyTableManager, cancellationToken);
			}
		}
		finally
		{
			if (shouldClose)
			{
				await connection.CloseAsync();
			}
		}
	}

	/// <summary>
	/// 	Creates history infrastructure (tables, functions, and triggers) for all history-enabled entities.
	/// 	This should be called after Database.EnsureCreated() or Database.Migrate().
	/// </summary>
	/// <param name="context">The DbContext</param>
	/// <param name="serviceProvider">The service provider</param>
	public static void CreateHistoryInfrastructure(
		this DbContext context,
		IServiceProvider serviceProvider)
    {
        var historyTableManager = serviceProvider.GetService(typeof(PostgreSqlHistoryTableManager)) as PostgreSqlHistoryTableManager;
        if (historyTableManager == null)
        {
            throw new InvalidOperationException("PostgreSqlHistoryTableManager is not registered in the service provider.");
        }

        var connection = context.Database.GetDbConnection();
        var shouldClose = false;

        if (connection.State == System.Data.ConnectionState.Closed)
        {
            connection.Open();
            shouldClose = true;
        }

        try
        {
            var historyEnabledEntities = context.Model.GetEntityTypes()
                                                      .Where(e => e.ClrType
                                                                   .GetCustomAttributes(typeof(HistoryEnabledAttribute), true)
                                                                   .Any())
                                                      .ToList();

            foreach (var entityType in historyEnabledEntities)
            {
                CreateHistoryInfrastructure(connection, entityType, historyTableManager);
            }
        }
        finally
        {
            if (shouldClose)
            {
                connection.Close();
            }
        }
    }

    private static async Task CreateHistoryInfrastructureAsync(
        System.Data.Common.DbConnection connection,
        Microsoft.EntityFrameworkCore.Metadata.IEntityType entityType,
        PostgreSqlHistoryTableManager historyTableManager,
        CancellationToken cancellationToken)
    {
        var tableName = entityType.GetTableName();
        var schema = entityType.GetSchema();
        var triggerName = $"{tableName}_history_trigger";

        // 1. Create the history table
        var tableCreationSql = historyTableManager.GenerateHistoryTableSql(entityType);
        using var tableCommand = connection.CreateCommand();
        tableCommand.CommandText = tableCreationSql;
        await tableCommand.ExecuteNonQueryAsync(cancellationToken);

        // 2. Create the trigger function
        var functionSql = historyTableManager.GenerateHistoryTriggerFunctionSql(entityType);
        using var functionCommand = connection.CreateCommand();
        functionCommand.CommandText = functionSql;
        await functionCommand.ExecuteNonQueryAsync(cancellationToken);

        // 3. Create the trigger using conditional SQL
        var schemaCondition = string.IsNullOrEmpty(schema) ? "" : $"AND table_schema = '{schema}'";
        var qualifiedTable = string.IsNullOrEmpty(schema) ? $"\"{tableName}\"" : $"\"{schema}\".\"{tableName}\"";
        var quotedTriggerName = $"\"{triggerName}\"";

        var triggerSql = $@"
DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = '{tableName}' {schemaCondition}) THEN
        IF NOT EXISTS (SELECT 1 FROM information_schema.triggers WHERE trigger_name = '{triggerName}' AND event_object_table = '{tableName}') THEN
            BEGIN
                EXECUTE 'DROP TRIGGER IF EXISTS {quotedTriggerName} ON {qualifiedTable}';
                EXECUTE 'CREATE TRIGGER {quotedTriggerName} BEFORE UPDATE OR DELETE ON {qualifiedTable} FOR EACH ROW EXECUTE FUNCTION {tableName}_history_trigger_func()';
            END;
        END IF;
    END IF;
END $$;";

        using var triggerCommand = connection.CreateCommand();
        triggerCommand.CommandText = triggerSql;
        await triggerCommand.ExecuteNonQueryAsync(cancellationToken);
    }

    private static void CreateHistoryInfrastructure(
        System.Data.Common.DbConnection connection,
        Microsoft.EntityFrameworkCore.Metadata.IEntityType entityType,
        PostgreSqlHistoryTableManager historyTableManager)
    {
        var tableName = entityType.GetTableName();
        var schema = entityType.GetSchema();
        var triggerName = $"{tableName}_history_trigger";

        // 1. Create the history table
        var tableCreationSql = historyTableManager.GenerateHistoryTableSql(entityType);
        using var tableCommand = connection.CreateCommand();
        tableCommand.CommandText = tableCreationSql;
        tableCommand.ExecuteNonQuery();

        // 2. Create the trigger function
        var functionSql = historyTableManager.GenerateHistoryTriggerFunctionSql(entityType);
        using var functionCommand = connection.CreateCommand();
        functionCommand.CommandText = functionSql;
        functionCommand.ExecuteNonQuery();

        // 3. Create the trigger using conditional SQL
        var schemaCondition = string.IsNullOrEmpty(schema) ? "" : $"AND table_schema = '{schema}'";
        var qualifiedTable = string.IsNullOrEmpty(schema) ? $"\"{tableName}\"" : $"\"{schema}\".\"{tableName}\"";
        var quotedTriggerName = $"\"{triggerName}\"";

        var triggerSql = $@"
DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = '{tableName}' {schemaCondition}) THEN
        IF NOT EXISTS (SELECT 1 FROM information_schema.triggers WHERE trigger_name = '{triggerName}' AND event_object_table = '{tableName}') THEN
            BEGIN
                EXECUTE 'DROP TRIGGER IF EXISTS {quotedTriggerName} ON {qualifiedTable}';
                EXECUTE 'CREATE TRIGGER {quotedTriggerName} BEFORE UPDATE OR DELETE ON {qualifiedTable} FOR EACH ROW EXECUTE FUNCTION {tableName}_history_trigger_func()';
            END;
        END IF;
    END IF;
END $$;";

        using var triggerCommand = connection.CreateCommand();
        triggerCommand.CommandText = triggerSql;
        triggerCommand.ExecuteNonQuery();
    }
}
