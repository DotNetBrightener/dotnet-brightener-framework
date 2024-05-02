using Microsoft.EntityFrameworkCore;

namespace DotNetBrightener.DataAccess.DataMigration;

internal class DataMigrationDbContext(DbContextOptions<DataMigrationDbContext> options) : DbContext(options)
{
    internal const string SchemaName = "DataMigration";

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DataMigrationHistory>(entity =>
        {
            entity.ToTable("__DataMigrationsHistory", SchemaName);

            entity.HasKey(e => e.MigrationId);
        });
    }
}