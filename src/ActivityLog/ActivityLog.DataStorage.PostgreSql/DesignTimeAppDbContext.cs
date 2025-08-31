using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ActivityLog.DataStorage.PostgreSql;

public class DesignTimeAppDbContext : IDesignTimeDbContextFactory<PostgreSqlMigrationDbContext>
{
    public PostgreSqlMigrationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<PostgreSqlMigrationDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Database=ActivityLogDesignTime;Username=postgres;Password=password");

        return new PostgreSqlMigrationDbContext(optionsBuilder.Options);
    }
}
