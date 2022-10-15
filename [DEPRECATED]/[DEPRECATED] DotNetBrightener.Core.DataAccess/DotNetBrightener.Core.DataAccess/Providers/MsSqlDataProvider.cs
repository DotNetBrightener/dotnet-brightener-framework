using DotNetBrightener.Core.DataAccess.Abstractions;
using LinqToDB;
using LinqToDB.DataProvider.SqlServer;
using Microsoft.Data.SqlClient;
using System;
using System.Data.Common;
using System.Threading;

namespace DotNetBrightener.Core.DataAccess.Providers
{
    public class MsSqlDataProvider : BaseDataProvider, IDotNetBrightenerDataProvider
    {
        public DatabaseProvider SupportedDatabaseProvider => DatabaseProvider.MsSql;
                
        public MsSqlDataProvider()
        {
            LinqToDbDataProvider = SqlServerDataProvider.
        }

        public void CreateDatabase(string connectionString, string collation = "", int triesToConnect = 10)
        {
            if (DatabaseExists(connectionString))
                return;

            var builder = new SqlConnectionStringBuilder(connectionString);

            // gets database name
            var databaseName = builder.InitialCatalog;

            // now create connection string to 'master' dabatase. It always exists.
            builder.InitialCatalog = "master";

            using (var connection = GetInternalDbConnection(builder.ConnectionString))
            {
                var query = $"CREATE DATABASE [{databaseName}]";
                if (!string.IsNullOrWhiteSpace(collation))
                    query = $"{query} COLLATE {collation}";

                var command = connection.CreateCommand();
                command.CommandText = query;
                command.Connection.Open();

                command.ExecuteNonQuery();
            }

            //try connect
            if (triesToConnect <= 0)
                return;

            // wait for awhile before re connecting
            for (var i = 0; i <= triesToConnect; i++)
            {
                if (i == triesToConnect)
                    throw new Exception("Unable to connect to the new database. Please try one more time");

                if (!DatabaseExists(connectionString))
                    Thread.Sleep(1000);
                else
                    break;
            }
        }

        protected override DbConnection GetInternalDbConnection(string connectionString)
        {
            return new SqlConnection(connectionString);
        }
    }
}