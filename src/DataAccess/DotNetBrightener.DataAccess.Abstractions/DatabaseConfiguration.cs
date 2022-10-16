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
    ///     Indicates whether Lazy loading is enabled for manipulating the records fetched from database
    /// </summary>
    public bool UseLazyLoading { get; set; } = true;

    /// <summary>
    ///     The database provider
    /// </summary>
    public DatabaseProvider DatabaseProvider { get; set; } = DatabaseProvider.MsSql;

    public static DatabaseConfiguration InitFromConfiguration(IConfiguration configuration,
                                                              string connectionStringName = "DatabaseConnectionString")
    {
        var dbConfig = new DatabaseConfiguration
        {
            ConnectionString = configuration.GetConnectionString(connectionStringName),
            DatabaseProvider = configuration.GetValue<DatabaseProvider>(nameof(DatabaseProvider))
        };

        return dbConfig;
    }
}