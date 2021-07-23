using System;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Design
{
    public abstract class SqlServerDbContextDesignTimeFactory<TDbContext> : IDesignTimeDbContextFactory<TDbContext>
        where TDbContext : DbContext
    {
        const string DefaultConnectionString =
            "Data Source=.;Initial Catalog=__;User ID=__;Password=__;MultipleActiveResultSets=True";

        public TDbContext CreateDbContext(string[] args)
        {
            var dbContextOptionBuilder = new DbContextOptionsBuilder<TDbContext>();

            dbContextOptionBuilder.UseSqlServer(DefaultConnectionString);

            return Activator.CreateInstance(typeof(TDbContext),
                                            new object[]
                                            {
                                                dbContextOptionBuilder.Options
                                            }) as
                       TDbContext;
        }
    }
}