using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DotNetBrightener.Integration.DataMigration.Internal
{
    public class DesignTimeDataMigrationDbContext : IDesignTimeDbContextFactory<DataMigrationMsSQLDbContext>
    {
        public DataMigrationMsSQLDbContext CreateDbContext(string[] args)
        {
            const string defaultConnectionString = "Data Source=.;Initial Catalog=__;User ID=__;Password=__;MultipleActiveResultSets=True";
            var optionsBuilder = new DbContextOptionsBuilder<DataMigrationMsSQLDbContext>();

            optionsBuilder.UseSqlServer(defaultConnectionString);

            return new DataMigrationMsSQLDbContext(optionsBuilder.Options);
        }
    }
}
