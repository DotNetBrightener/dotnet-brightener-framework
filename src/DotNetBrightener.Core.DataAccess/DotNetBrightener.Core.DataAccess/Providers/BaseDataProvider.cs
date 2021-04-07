using LinqToDB.Data;
using LinqToDB.DataProvider;
using System.Data.Common;

namespace DotNetBrightener.Core.DataAccess.Providers
{
    public abstract class BaseDataProvider
    {
        public IDataProvider LinqToDbDataProvider { get; protected set; }

        public DataConnection CreateDataConnection(string connectionString)
        {
            var dataContext = new DataConnection(LinqToDbDataProvider, GetInternalDbConnection(connectionString));

            return dataContext;
        }

        protected abstract DbConnection GetInternalDbConnection(string connectionString);

        public bool DatabaseExists(string connectionString)
        {
            try
            {
                using var connection = GetInternalDbConnection(connectionString);

                //just try to connect
                connection.Open();

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}