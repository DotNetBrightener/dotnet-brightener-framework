using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DotNetBrightener.Core.BackgroundTasks.Data.DbContexts;

internal class SqlServerDbContextDesignTimeFactory : IDesignTimeDbContextFactory<BackgroundTaskDbContext>
{
    private const string defaultConnectionString =
        "Data Source=.;Initial Catalog=__;User ID=__;Password=__;MultipleActiveResultSets=True";

    public BackgroundTaskDbContext CreateDbContext(string[] args)
    {
        var dbContextOptionBuilder = new DbContextOptionsBuilder<BackgroundTaskDbContext>();

        dbContextOptionBuilder.UseSqlServer(defaultConnectionString);

        return Activator.CreateInstance(typeof(BackgroundTaskDbContext),
                                        new object[]
                                        {
                                            dbContextOptionBuilder.Options
                                        }) as
                   BackgroundTaskDbContext;
    }
}