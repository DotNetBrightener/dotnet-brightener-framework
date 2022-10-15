using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DotNetBrightener.Integration.DataMigration.Internal
{
    public class DesignTimeDataMigrationPostgreDbContext : IDesignTimeDbContextFactory<DataMigrationPostgreSQLDbContext>
    {
        public DataMigrationPostgreSQLDbContext CreateDbContext(string[] args)
        {
            const string defaultConnectionString = "Data Source=.;Initial Catalog=__;User ID=__;Password=__;MultipleActiveResultSets=True";
            var optionsBuilder = new DbContextOptionsBuilder<DataMigrationPostgreSQLDbContext>();

            optionsBuilder.UseNpgsql(defaultConnectionString);

            return new DataMigrationPostgreSQLDbContext(optionsBuilder.Options);
        }
    }
}
