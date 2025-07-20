using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace DotNetBrightener.DataAccess.EF.PostgreSQL;

public static class PostgresFunctionInitializer
{
    private static          bool _isInitialized = false;
    private static readonly Lock Lock           = new();

    public static void InitializeUuidV7Function(DbContext context, string defaultDatabaseName = "postgres")
    {
        lock (Lock)
        {
            if (_isInitialized) return;

            var sql = @"
                CREATE EXTENSION IF NOT EXISTS ""pgcrypto"";

                CREATE OR REPLACE FUNCTION generate_uuid_v7()
                RETURNS UUID AS $$
                DECLARE
                    ts BIGINT;
                    rand BYTEA;
                    uuid BYTEA;
                BEGIN
                    -- Unix timestamp in milliseconds (48 bits = 12 hex chars)
                    ts := (EXTRACT(EPOCH FROM clock_timestamp()) * 1000)::BIGINT;

                    -- 10 random bytes
                    rand := gen_random_bytes(10);

                    -- Concatenate padded timestamp hex + 10 random bytes (20 hex chars)
                    uuid := set_byte(set_byte(  
                        decode(lpad(to_hex(ts), 12, '0') || encode(rand, 'hex'), 'hex'),
                        6,
                        (get_byte(decode(lpad(to_hex(ts), 12, '0') || encode(rand, 'hex'), 'hex'), 6) & 15) | 112 -- version 7
                    ), 8,
                        (get_byte(decode(lpad(to_hex(ts), 12, '0') || encode(rand, 'hex'), 'hex'), 8) & 63) | 128 -- variant
                    );

                    RETURN encode(uuid, 'hex')::uuid;
                END;
                $$ LANGUAGE plpgsql;
            ";

            try
            {
                context.Database.ExecuteSqlRaw(sql);
            }
            catch (Exception exception)
            {
                if (exception.Message.Contains("database") && exception.Message.EndsWith("does not exist"))
                {
                    try
                    {
                        var connectionString = context.Database.GetConnectionString();
                        EnsureDatabaseExists(connectionString, defaultDatabaseName).Wait();
                    }
                    catch (Exception innerException)
                    {
                        throw;
                    }
                }
                context.Database.ExecuteSqlRaw(sql);
            }
            _isInitialized = true;
        }
    }

    internal static async Task EnsureDatabaseExists(string connectionString, string defaultDatabaseName = "postgres")
    {
        var builder = new NpgsqlConnectionStringBuilder(connectionString);
        var targetDbName = builder.Database;

        builder.Database = defaultDatabaseName;
        var adminConnectionString = builder.ToString();

        using var connection = new NpgsqlConnection(adminConnectionString);
        connection.Open();

        using var checkCmd = new NpgsqlCommand($"SELECT 1 FROM pg_database WHERE datname = @dbname", connection);
        checkCmd.Parameters.AddWithValue("dbname", targetDbName);

        var exists = checkCmd.ExecuteScalar() != null;
        if (!exists)
        {
            using var createCmd = new NpgsqlCommand($"CREATE DATABASE \"{targetDbName}\"", connection);
            await createCmd.ExecuteNonQueryAsync();
        }
    }

    public static void ApplyUuidV7DefaultForGuidKeys(this ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var idProperty = entityType.FindProperty("Id");

            if (idProperty != null &&
                idProperty.ClrType == typeof(Guid) &&
                idProperty.IsPrimaryKey())
            {
                idProperty.SetDefaultValueSql("generate_uuid_v7()");
            }
        }
    }
}