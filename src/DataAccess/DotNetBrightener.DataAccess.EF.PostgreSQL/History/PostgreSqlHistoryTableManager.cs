#nullable enable
using DotNetBrightener.DataAccess.Attributes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging;
using System.Text;

namespace DotNetBrightener.DataAccess.EF.PostgreSQL.History;

/// <summary>
/// Manages PostgreSQL history tables and triggers for entities marked with HistoryEnabledAttribute
/// </summary>
internal class PostgreSqlHistoryTableManager
{
    private readonly ILogger _logger;

    public PostgreSqlHistoryTableManager(IServiceProvider serviceProvider)
    {
        var loggerFactory = serviceProvider.GetService(typeof(ILoggerFactory)) as ILoggerFactory;

        _logger = loggerFactory.CreateLogger<PostgreSqlHistoryTableManager>();
    }

    /// <summary>
    /// Configures history tables for entities marked with HistoryEnabledAttribute
    /// </summary>
    public void ConfigureHistoryTables(ModelBuilder modelBuilder)
    {
        var models = modelBuilder.Model
                                 .GetEntityTypes();
        
        var historyEnabledEntities = models
            .Where(x => x.ClrType.GetCustomAttributes(typeof(HistoryEnabledAttribute), true).Any())
            .ToList();

        foreach (var entityType in historyEnabledEntities)
        {
            ConfigureHistoryTableForEntity(modelBuilder, entityType);
        }
    }

    /// <summary>
    /// Configures history table for a specific entity
    /// </summary>
    private void ConfigureHistoryTableForEntity(ModelBuilder modelBuilder, IMutableEntityType entityType)
    {
        var tableName = entityType.GetTableName();
        var schema = entityType.GetSchema();
        var historyTableName = $"{tableName}_History";

        _logger.LogDebug("Configuring history table {HistoryTableName} for entity {EntityName}", 
            historyTableName, entityType.ClrType.Name);

        // Create history table entity configuration
        var historyEntityBuilder = modelBuilder.Entity(entityType.ClrType.Name + "History", builder =>
        {
            builder.ToTable(historyTableName, schema);
            
            // Copy all properties from the main entity
            foreach (var property in entityType.GetProperties())
            {
                if (property.IsShadowProperty())
                    continue;

                var propertyBuilder = builder.Property(property.ClrType, property.Name);
                
                // Copy property configuration
                if (property.GetMaxLength() != null)
                    propertyBuilder.HasMaxLength(property.GetMaxLength().Value);
                
                if (property.IsNullable)
                    propertyBuilder.IsRequired(false);
                else
                    propertyBuilder.IsRequired();

                // Handle column type mapping for PostgreSQL
                var columnType = GetPostgreSqlColumnType(property);
                if (!string.IsNullOrEmpty(columnType))
                    propertyBuilder.HasColumnType(columnType);
            }

            // Add period columns for history tracking
            builder.Property<DateTime>("PeriodStart")
                .HasColumnType("timestamptz")
                .IsRequired();

            builder.Property<DateTime>("PeriodEnd")
                .HasColumnType("timestamptz")
                .IsRequired();

            // Create composite primary key with original PK + PeriodStart
            var primaryKeyProperties = entityType.FindPrimaryKey()?.Properties
                .Select(p => p.Name)
                .Concat(new[] { "PeriodStart" })
                .ToArray();

            if (primaryKeyProperties?.Length > 0)
            {
                builder.HasKey(primaryKeyProperties);
            }

            // Create indexes for efficient querying
            builder.HasIndex("PeriodStart");
            builder.HasIndex("PeriodEnd");
        });

        // Store metadata for trigger generation
        entityType.SetAnnotation("PostgreSQL:HistoryTableName", historyTableName);
        entityType.SetAnnotation("PostgreSQL:HistoryTableSchema", schema);
    }

    /// <summary>
    ///     Maps EF property types to PostgreSQL column types
    /// </summary>
    private string GetPostgreSqlColumnType(IMutableProperty property)
    {
        var clrType        = property.ClrType;
        var underlyingType = Nullable.GetUnderlyingType(clrType) ?? clrType;

        return underlyingType.Name switch
        {
            nameof(DateTime)                                    => "timestamptz",
            nameof(DateTimeOffset)                              => "timestamptz",
            nameof(TimeSpan)                                    => "interval",
            nameof(Guid)                                        => "uuid",
            nameof(Boolean)                                     => "boolean",
            nameof(Int16)                                       => "smallint",
            nameof(Int32)                                       => "integer",
            nameof(Int64)                                       => "bigint",
            nameof(Single)                                      => "real",
            nameof(Double)                                      => "double precision",
            nameof(Decimal)                                     => "numeric",
            nameof(String) when property.GetMaxLength() != null => $"varchar({property.GetMaxLength()})",
            nameof(String)                                      => "text",
            _                                                   => string.Empty
        };
    }

    /// <summary>
    ///     Generates SQL for creating history triggers
    /// </summary>
    public string GenerateHistoryTriggerSql(IEntityType entityType)
    {
        var tableName = entityType.GetTableName();
        var schema = entityType.GetSchema();
        var historyTableName = $"{tableName}_History";
        var triggerName = $"{tableName}_history_trigger";
        var functionName = $"{tableName}_history_function";

        var fullTableName = string.IsNullOrEmpty(schema) ? tableName : $"{schema}.{tableName}";
        var fullHistoryTableName = string.IsNullOrEmpty(schema) ? historyTableName : $"{schema}.{historyTableName}";

        var columns = entityType.GetProperties()
            .Where(p => !p.IsShadowProperty())
            .Select(p => p.Name)
            .ToList();

        var columnList = string.Join(", ", columns);
        var oldColumnList = string.Join(", ", columns.Select(c => $"OLD.{c}"));

        var sql = new StringBuilder();

        // Create trigger function
        sql.AppendLine($@"
CREATE OR REPLACE FUNCTION {functionName}()
RETURNS TRIGGER AS $$
BEGIN
    IF TG_OP = 'DELETE' THEN
        INSERT INTO {fullHistoryTableName} ({columnList}, PeriodStart, PeriodEnd)
        VALUES ({oldColumnList}, 
                COALESCE(OLD.""ModifiedDate"", OLD.""CreatedDate"", NOW()), 
                NOW());
        RETURN OLD;
    ELSIF TG_OP = 'UPDATE' THEN
        INSERT INTO {fullHistoryTableName} ({columnList}, PeriodStart, PeriodEnd)
        VALUES ({oldColumnList}, 
                COALESCE(OLD.""ModifiedDate"", OLD.""CreatedDate"", NOW()), 
                COALESCE(NEW.""ModifiedDate"", NEW.""CreatedDate"", NOW()));
        RETURN NEW;
    END IF;
    RETURN NULL;
END;
$$ LANGUAGE plpgsql;");

        // Create trigger
        sql.AppendLine($@"
DROP TRIGGER IF EXISTS {triggerName} ON {fullTableName};
CREATE TRIGGER {triggerName}
    BEFORE UPDATE OR DELETE ON {fullTableName}
    FOR EACH ROW EXECUTE FUNCTION {functionName}();");

        return sql.ToString();
    }
}
