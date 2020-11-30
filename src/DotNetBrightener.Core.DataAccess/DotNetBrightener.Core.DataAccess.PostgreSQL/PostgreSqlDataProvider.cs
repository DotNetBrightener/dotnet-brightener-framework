using DotNetBrightener.Core.DataAccess.Providers;
using LinqToDB.Data;
using LinqToDB.DataProvider;
using LinqToDB.DataProvider.PostgreSQL;
using Npgsql;

namespace DotNetBrightener.Core.DataAccess.PostgreSQL
{
    public class PostgreSqlDataProvider : IDotNetBrightenerDataProvider
    {
        public DatabaseProvider SupportedDatabaseProvider => DatabaseProvider.PostgreSql;

        public IDataProvider LinqToDbDataProvider { get; }

        public PostgreSqlDataProvider()
        {
            LinqToDbDataProvider = new PostgreSQLDataProvider();
        }

        public DataConnection CreateDataConnection(string connectionString)
        {
            var dataContext = new DataConnection(LinqToDbDataProvider, new NpgsqlConnection(connectionString));

            return dataContext;
        }
    }
}