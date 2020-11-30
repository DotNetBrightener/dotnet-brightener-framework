using LinqToDB;
using LinqToDB.Data;
using LinqToDB.DataProvider;
using LinqToDB.DataProvider.SqlServer;
using Microsoft.Data.SqlClient;

namespace DotNetBrightener.Core.DataAccess.Providers
{
    public interface IDotNetBrightenerDataProvider
    {
        DatabaseProvider SupportedDatabaseProvider { get; }

        DataConnection CreateDataConnection(string connectionString);
    }

    public class MsSqlDataProvider : IDotNetBrightenerDataProvider
    {
        public DatabaseProvider SupportedDatabaseProvider => DatabaseProvider.MsSql;

        public IDataProvider LinqToDbDataProvider { get; }
        
        public MsSqlDataProvider()
        {
            LinqToDbDataProvider = new SqlServerDataProvider(ProviderName.SqlServer, SqlServerVersion.v2008);
        }

        public DataConnection CreateDataConnection(string connectionString)
        {
            var dataContext = new DataConnection(LinqToDbDataProvider, new SqlConnection(connectionString));
            
            return dataContext;
        }
    }
}