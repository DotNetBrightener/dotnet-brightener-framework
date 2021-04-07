using DotNetBrightener.Core.DataAccess.Abstractions;
using LinqToDB.Data;

namespace DotNetBrightener.Core.DataAccess.Providers
{
    public interface IDotNetBrightenerDataProvider
    {
        DatabaseProvider SupportedDatabaseProvider { get; }

        DataConnection CreateDataConnection(string connectionString);

        bool DatabaseExists(string connectionString);

        void CreateDatabase(string connectionString, string collation = "", int triesToConnect = 10);
    }
}