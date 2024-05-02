using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DotNetBrightener.DataAccess.DataMigration.PostgreSql;

internal class PostgreSqlDbContextDesignTimeFactory : IDesignTimeDbContextFactory<DataMigrationDbContext>
{
    const string defaultConnectionString =
        "Data Source=.;Initial Catalog=__;User ID=__;Password=__;MultipleActiveResultSets=True";

    public DataMigrationDbContext CreateDbContext(string[] args)
    {
        var dbContextOptionBuilder = new DbContextOptionsBuilder<DataMigrationDbContext>();

        dbContextOptionBuilder.UseNpgsql(defaultConnectionString,
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