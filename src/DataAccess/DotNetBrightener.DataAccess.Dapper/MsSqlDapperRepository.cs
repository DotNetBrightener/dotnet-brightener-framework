using System.Data;
using Dapper;
using DotNetBrightener.DataAccess.Dapper.Abstractions;
using Microsoft.Data.SqlClient;

namespace DotNetBrightener.DataAccess.Dapper;

public class MsSqlDapperRepository(
    IServiceProvider      serviceProvider,
    DatabaseConfiguration databaseConfiguration) : IDapperRepository
{
    private readonly Func<IServiceProvider, string, IDbConnection> _connectionFactory = InitializeConnection;

    public MsSqlDapperRepository(IServiceProvider                              serviceProvider,
                                 DatabaseConfiguration                         databaseConfiguration,
                                 Func<IServiceProvider, string, IDbConnection> connectionFactory)
        : this(serviceProvider, databaseConfiguration)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IQueryable<TEntity>> FetchEntities<TEntity>(string sqlQuery, object param = null)
    {
        using (var connection = await NewConnection())
        {
            var result = await connection.QueryAsync<TEntity>(sqlQuery, param);

            return result.AsQueryable();
        }
    }

    public async Task<TEntity> GetEntity<TEntity>(string sqlQuery, object param = null)
    {
        using (var connection = await NewConnection())
        {
            var result = await connection.QueryFirstOrDefaultAsync<TEntity>(sqlQuery, param);

            return result;
        }
    }

    public async Task<TEntity> ExecuteScalar<TEntity>(string sqlQuery, object param = null)
    {
        using (var connection = await NewConnection())
        {
            var result = await connection.ExecuteScalarAsync<TEntity>(sqlQuery, param);

            return result;
        }
    }

    private async Task<IDbConnection> NewConnection()
    {
        var connection = _connectionFactory(serviceProvider,
                                            databaseConfiguration.ConnectionString);

        if (connection is SqlConnection sqlConnection)
        {
            await sqlConnection.OpenAsync();
        }
        else
        {
            connection.Open();
        }

        return connection;
    }

    private static IDbConnection InitializeConnection(IServiceProvider serviceProvider,
                                                      string           baseConnectionString)
    {
        var connection = new SqlConnection(baseConnectionString);

        return connection;
    }
}