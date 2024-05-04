using Microsoft.EntityFrameworkCore.Design;

namespace DotNetBrightener.Infrastructure.AppClientManager.DataStorage.Mssql;

internal class DesignTimeAppDbContext : SqlServerDbContextDesignTimeFactory<SqlServerMigrationDbContext>
{
}