using DotNetBrightener.Core.DataAccess.Abstractions;
using DotNetBrightener.Core.DataAccess.Providers;
using LinqToDB.DataProvider.PostgreSQL;
using Npgsql;
using System;
using System.Data.Common;
using System.Threading;

namespace DotNetBrightener.Core.DataAccess.PostgreSQL
{
    public class PostgreSqlDataProvider : BaseDataProvider, IDotNetBrightenerDataProvider
    {
        public DatabaseProvider SupportedDatabaseProvider => DatabaseProvider.PostgreSql;

        public PostgreSqlDataProvider()
        {
            LinqToDbDataProvider = new PostgreSQLDataProvider();
        }

        public void CreateDatabase(string connectionString, string collation = "", int triesToConnect = 10)
        {
            if (DatabaseExists(connectionString))
                return;

            var builder = new NpgsqlConnectionStringBuilder(connectionString);

            // gets database name
            var databaseName = builder.Database;

            // now create connection string to 'postgres' - default administrative connection database.
            builder.Database = "postgres";

            using (var connection = GetInternalDbConnection(builder.ConnectionString))
            {
                var query = $"CREATE DATABASE \"{databaseName}\" WITH OWNER = '{builder.Username}'";
                if (!string.IsNullOrWhiteSpace(collation))
                    query = $"{query} LC_COLLATE = '{collation}'";

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
                {
                    builder.Database = databaseName;
                    using var connection = GetInternalDbConnection(builder.ConnectionString) as NpgsqlConnection;
                    var command = connection.CreateCommand();
                    command.CommandText = "CREATE EXTENSION IF NOT EXISTS citext; CREATE EXTENSION IF NOT EXISTS pgcrypto;";
                    command.Connection.Open();
                    command.ExecuteNonQuery();
                    connection.ReloadTypes();

                    break;
                }
            }
        }

        protected override DbConnection GetInternalDbConnection(string connectionString)
        {
            return new NpgsqlConnection(connectionString);
        }
    }
}