using Microsoft.Extensions.Configuration;

namespace DotNetBrightener.DataAccess;

/// <summary>
///     Represents the configuration of how to connect to the database
/// </summary>
public class DatabaseConfiguration
{
    /// <summary>
    ///     The connection string to connect to the database
    /// </summary>
    public string ConnectionString { get; set; }


    /// <summary>
    ///     The connection string to connect to the Audit Database
    /// </summary>
    public string AuditDbConnectionString { get; set; }

    /// <summary>
    ///     Indicates whether the system should use Audit
    /// </summary>
    public bool UseAudit { get; set; } = false;

    /// <summary>
    ///     Indicates whether Lazy loading is enabled for manipulating the records fetched from database
    /// </summary>
    public bool UseLazyLoading { get; set; } = true;

    /// <summary>
    ///     The database provider for Application Database
    /// </summary>
    public DatabaseProvider DatabaseProvider { get; set; } = DatabaseProvider.MsSql;

    /// <summary>
    ///     The database provider for Auditing Database
    /// </summary>
    public DatabaseProvider AuditDatabaseProvider { get; set; } = DatabaseProvider.MsSql;

    public static DatabaseConfiguration InitFromConfiguration(IConfiguration configuration,
                                                              string connectionStringName = "DatabaseConnectionString",
                                                              string auditConnectionStringName = "DatabaseConnectionString")
    {
        var dbConfig = new DatabaseConfiguration
        {
            ConnectionString        = configuration.GetConnectionString(connectionStringName),
            DatabaseProvider        = configuration.GetValue<DatabaseProvider>(nameof(DatabaseProvider)),
            AuditDbConnectionString = configuration.GetConnectionString(auditConnectionStringName),
            AuditDatabaseProvider   = configuration.GetValue<DatabaseProvider>(nameof(AuditDatabaseProvider))
        };

        return dbConfig;
    }
}