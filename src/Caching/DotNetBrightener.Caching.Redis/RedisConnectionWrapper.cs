using System.Net;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace DotNetBrightener.Caching.Redis;

/// <summary>
///     Represents Redis connection wrapper implementation
/// </summary>
public class RedisConnectionWrapper : IRedisConnectionWrapper
{
    private readonly RedisCacheConfiguration _redisCacheConfiguration;

    #region Fields

    private          bool                  _disposed = false;
    private readonly object                _lock     = new object();
    private volatile ConnectionMultiplexer _connection;

    #endregion

    #region Ctor

    public RedisConnectionWrapper(IOptions<RedisCacheConfiguration> redisCacheConfiguration)
    {
        _redisCacheConfiguration = redisCacheConfiguration.Value;
    }

    #endregion

    #region Utilities
        
    /// <summary>
    /// Get connection to Redis servers
    /// </summary>
    /// <returns></returns>
    protected ConnectionMultiplexer GetConnection()
    {
        if (_connection != null && _connection.IsConnected) 
            return _connection;

        lock (_lock)
        {
            if (_connection != null && _connection.IsConnected) 
                return _connection;

            //Connection disconnected. Disposing connection...
            _connection?.Dispose();

            var configurationOptions = new ConfigurationOptions
            {
                DefaultDatabase    = _redisCacheConfiguration.DefaultDatabase,
                Password           = _redisCacheConfiguration.RedisPassword,
                EndPoints          = { { _redisCacheConfiguration.ServerAddress, _redisCacheConfiguration.RedisPort } },
                KeepAlive          = _redisCacheConfiguration.KeepAlive,
                DefaultVersion     = new Version(1, 1, 1),
                AbortOnConnectFail = false
            };

            //Creating new instance of Redis Connection
            _connection = ConnectionMultiplexer.Connect(configurationOptions);
        }

        return _connection;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Obtain an interactive connection to a database inside Redis
    /// </summary>
    /// <param name="db">Database number</param>
    /// <returns>Redis cache database</returns>
    public IDatabase GetDatabase(int db)
    {
        return GetConnection().GetDatabase(db);
    }

    /// <summary>
    /// Obtain a configuration API for an individual server
    /// </summary>
    /// <param name="endPoint">The network endpoint</param>
    /// <returns>Redis server</returns>
    public IServer GetServer(EndPoint endPoint)
    {
        return GetConnection().GetServer(endPoint);
    }

    /// <summary>
    /// Gets all endpoints defined on the server
    /// </summary>
    /// <returns>Array of endpoints</returns>
    public EndPoint[] GetEndPoints()
    {
        return GetConnection().GetEndPoints();
    }

    /// <summary>
    /// Delete all the keys of the database
    /// </summary>
    /// <param name="db">Database number</param>
    public void FlushDatabase(RedisDatabaseNumber db)
    {
        var endPoints = GetEndPoints();

        foreach (var endPoint in endPoints)
        {
            GetServer(endPoint).FlushDatabase((int)db);
        }
    }

    /// <summary>
    /// Release all resources associated with this object
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    // Protected implementation of Dispose pattern.
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            //dispose ConnectionMultiplexer
            _connection?.Dispose();
        }
        _disposed = true;
    }

    #endregion
}