using DotNetBrightener.Core.DataAccess.Abstractions;
using FluentMigrator.Runner.Initialization;

namespace DotNetBrightener.Core.DataAccess.Migration.Extensions
{
    internal class DataConnectionStringAccessor : IConnectionStringAccessor
    {
        public DataConnectionStringAccessor(DatabaseConfiguration databaseConfiguration)
        {
            ConnectionString = databaseConfiguration.ConnectionString;
        }

        public string ConnectionString { get; private set; }
    }
}
