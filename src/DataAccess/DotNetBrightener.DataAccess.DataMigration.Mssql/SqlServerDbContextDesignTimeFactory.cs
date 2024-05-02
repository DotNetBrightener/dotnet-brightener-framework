using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System.Reflection;

namespace DotNetBrightener.DataAccess.DataMigration.Mssql;

internal class SqlServerDbContextDesignTimeFactory : IDesignTimeDbContextFactory<DataMigrationDbContext>
{
    const string defaultConnectionString =
        "Data Source=.;Initial Catalog=__;User ID=__;Password=__;MultipleActiveResultSets=True";

    public DataMigrationDbContext CreateDbContext(string[] args)
    {
        var dbContextOptionBuilder = new DbContextOptionsBuilder<DataMigrationDbContext>();

        dbContextOptionBuilder.UseSqlServer(defaultConnectionString,
                                            x =>
                                            {
                                                x.MigrationsAssembly(Assembly.GetExecutingAssembly().FullName);
                                            });

        return Activator.CreateInstance(typeof(DataMigrationDbContext),
                                        new object[]
                                        {
                                            dbContextOptionBuilder.Options
                                        }) as
                   DataMigrationDbContext;
    }
}