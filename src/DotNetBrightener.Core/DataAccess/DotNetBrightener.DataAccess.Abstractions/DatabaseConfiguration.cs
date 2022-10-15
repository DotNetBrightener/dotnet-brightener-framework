using Microsoft.Extensions.Configuration;

namespace DotNetBrightener.DataAccess;

public class DatabaseConfiguration
{
    public string ConnectionString { get; set; }

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