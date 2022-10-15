using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using DotNetBrightener.DataAccess.Dapper.Abstractions;
using Npgsql;

namespace DotNetBrightener.DataAccess.Dapper.PostgreSQL;

public class NpgsqlDapperRepository : IDapperRepository
{
    private readonly DatabaseConfiguration _databaseConfiguration;

    public NpgsqlDapperRepository(DatabaseConfiguration databaseConfiguration)
    {
        _databaseConfiguration = databaseConfiguration;
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

    private Task<IDbConnection> NewConnection()
    {
        return NewConnection(_databaseConfiguration.ConnectionString);
    }

    private static async Task<IDbConnection> NewConnection(string connectionString)
    {
        var connection = new NpgsqlConnection(connectionString);

        connection.Open();

        return connection;
    }
}