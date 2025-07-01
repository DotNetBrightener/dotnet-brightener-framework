// ReSharper disable once CheckNamespace
namespace Testcontainers.PostgreSql;

public static class PostgreSqlContainerGenerator
{
    public static PostgreSqlContainer CreateContainer(string databaseName = "",
                                                      int?   port         = null)
    {
        var postgreSqlBuilder = new PostgreSqlBuilder()
                               .WithImage("postgres:17")
                               .WithDatabase(databaseName)
                               .WithUsername("test")
                               .WithPassword("password");

        if (port.HasValue)
        {
            postgreSqlBuilder = postgreSqlBuilder.WithPortBinding(port.Value, PostgreSqlBuilder.PostgreSqlPort);
        }

        var container = postgreSqlBuilder.Build();

        return container;
    }
}