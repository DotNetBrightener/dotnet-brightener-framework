using DotNet.Testcontainers.Builders;

// ReSharper disable once CheckNamespace
namespace Testcontainers.MsSql;

public static class MsSqlContainerGenerator
{
    public static MsSqlContainer CreateContainer(string containerName = "")
    {
        var builder = new MsSqlBuilder()
                     .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
                     .WithPassword("Str0ng3stP@s5w0rd3ver!")
                     .WithWaitStrategy(Wait.ForUnixContainer()
                                           .UntilMessageIsLogged("SQL Server is now ready for client connections"));

        if (!string.IsNullOrWhiteSpace(containerName))
        {
            builder.WithName(containerName);
        }

        return builder.Build();
    }
}