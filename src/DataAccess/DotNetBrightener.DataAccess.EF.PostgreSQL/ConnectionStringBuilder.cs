using System;
using Npgsql;

namespace DotNetBrightener.DataAccess.EF.PostgreSQL;

public static class ConnectionStringBuilder
{
    public static DatabaseConfiguration
        BuildConfiguration(string               connectionString,
                           ConnectionStringType connType = ConnectionStringType.Plain)
    {
        var databaseConfig = new DatabaseConfiguration
        {
            DatabaseProvider = DatabaseProvider.PostgreSql
        };

        if (connType == ConnectionStringType.Plain ||
            // cannot parse the URI => consider as plain connection string
            // connection URI should be an absolute path
            !Uri.TryCreate(connectionString, UriKind.Absolute, out var databaseUri))
        {
            databaseConfig.ConnectionString = connectionString;

            return databaseConfig;
        }

        var userInfo = databaseUri.UserInfo.Split(':');

        var builder = new NpgsqlConnectionStringBuilder
        {
            Host                   = databaseUri.Host,
            Port                   = databaseUri.Port,
            Username               = userInfo[0],
            Password               = userInfo[1],
            Database               = databaseUri.LocalPath.TrimStart('/'),
            SslMode                = SslMode.Prefer, // TODO: how to pick up this config?
            TrustServerCertificate = true            // TODO: How to configure?
        };

        databaseConfig.ConnectionString = builder.ToString();

        return databaseConfig;
    }
}