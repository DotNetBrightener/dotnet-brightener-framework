// ReSharper disable once CheckNamespace
namespace Testcontainers.MsSql;

public static class MsSqlContainerExtensions
{
    public static string GetConnectionString(this MsSqlContainer container, string databaseName = "TestDb")
    {
        return container.GetConnectionString()
                        .Replace("Database=master", databaseName);
    }
}