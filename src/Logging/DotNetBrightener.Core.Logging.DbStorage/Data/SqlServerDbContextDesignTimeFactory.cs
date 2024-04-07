using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DotNetBrightener.Core.Logging.DbStorage.Data;

internal class SqlServerDbContextDesignTimeFactory : IDesignTimeDbContextFactory<LoggingDbContext>
{
    const string defaultConnectionString =
        "Data Source=.;Initial Catalog=__;User ID=__;Password=__;MultipleActiveResultSets=True";

    public LoggingDbContext CreateDbContext(string[] args)
    {
        var dbContextOptionBuilder = new DbContextOptionsBuilder<LoggingDbContext>();

        dbContextOptionBuilder.UseSqlServer(defaultConnectionString);

        return Activator.CreateInstance(typeof(LoggingDbContext),
                                        new object[]
                                        {
                                            dbContextOptionBuilder.Options
                                        }) as
                   LoggingDbContext;
    }
}