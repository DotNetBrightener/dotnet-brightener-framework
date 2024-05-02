using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DotNetBrightener.DataAccess.DataMigration;

internal class SqlServerDbContextDesignTimeFactory : IDesignTimeDbContextFactory<DataMigrationDbContext>
{
    const string defaultConnectionString =
        "Data Source=.;Initial Catalog=__;User ID=__;Password=__;MultipleActiveResultSets=True";

    public DataMigrationDbContext CreateDbContext(string[] args)
    {
        var dbContextOptionBuilder = new DbContextOptionsBuilder<DataMigrationDbContext>();

        dbContextOptionBuilder.UseSqlServer(defaultConnectionString);

        return Activator.CreateInstance(typeof(DataMigrationDbContext),
                                        new object[]
                                        {
                                            dbContextOptionBuilder.Options
                                        }) as
                   DataMigrationDbContext;
    }
}

internal class DataMigrationDbContext(DbContextOptions<DataMigrationDbContext> options) : DbContext(options)
{
    internal const string SchemaName = "DataMigration";

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DataMigrationHistory>(entity =>
        {
            entity.ToTable("__DataMigrationsHistory", SchemaName);

            entity.HasKey(e => e.MigrationId);

            entity.Property(e => e.MigrationId)
                  .HasMaxLength(150);

            entity.Property(e => e.AppliedDateUtc)
                  .IsRequired()
                  .HasDefaultValueSql("GETUTCDATE()");
        });
    }
}