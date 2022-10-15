using System;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Design;

public abstract class SqlServerDbContextDesignTimeFactory<TDbContext> : IDesignTimeDbContextFactory<TDbContext>
    where TDbContext : DbContext
{
    const string defaultConnectionString =
        "Data Source=.;Initial Catalog=__;User ID=__;Password=__;MultipleActiveResultSets=True";

    public TDbContext CreateDbContext(string[] args)
    {
        var dbContextOptionBuilder = new DbContextOptionsBuilder<TDbContext>();

        dbContextOptionBuilder.UseSqlServer(defaultConnectionString);

        return Activator.CreateInstance(typeof(TDbContext), new object[] {dbContextOptionBuilder.Options}) as
                   TDbContext;
    }
}