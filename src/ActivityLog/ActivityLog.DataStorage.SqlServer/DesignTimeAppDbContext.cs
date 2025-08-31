using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ActivityLog.DataStorage.SqlServer;

public class DesignTimeAppDbContext : IDesignTimeDbContextFactory<SqlServerMigrationDbContext>
{
    public SqlServerMigrationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<SqlServerMigrationDbContext>();
        optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=ActivityLogDesignTime;Trusted_Connection=true;MultipleActiveResultSets=true");

        return new SqlServerMigrationDbContext(optionsBuilder.Options);
    }
}
