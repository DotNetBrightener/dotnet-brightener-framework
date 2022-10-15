using DotNetBrightener.Core.DataAccess.Abstractions;
using System.Collections.Generic;
using System.Linq;

namespace DotNetBrightener.Core.DataAccess.Providers
{
    public interface IDataProviderFactory
    {
        IDotNetBrightenerDataProvider GetDataProvider();

        bool DatabaseExists();

        void CreateDatabase(string collation = "", int triesToConnect = 10);
    }

    public class DataProviderFactory : IDataProviderFactory
    {
        private readonly DatabaseConfiguration _databaseConfiguration;
        private readonly IEnumerable<IDotNetBrightenerDataProvider> _dataProviders;

        public DataProviderFactory(DatabaseConfiguration databaseConfiguration,
                                   IEnumerable<IDotNetBrightenerDataProvider> dataProviders)
        {
            _databaseConfiguration = databaseConfiguration;
            _dataProviders = dataProviders;
        }

        public void CreateDatabase(string collation = "", int triesToConnect = 10)
        {
            GetDataProvider().CreateDatabase(_databaseConfiguration.ConnectionString, collation, triesToConnect);
        }

        public bool DatabaseExists()
        {
            return GetDataProvider().DatabaseExists(_databaseConfiguration.ConnectionString);
        }

        public IDotNetBrightenerDataProvider GetDataProvider()
        {
            return _dataProviders.FirstOrDefault(_ => _.SupportedDatabaseProvider == _databaseConfiguration.DatabaseProvider);
        }
    }
}