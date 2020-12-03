using Microsoft.EntityFrameworkCore;

namespace DotNetBrightener.Integration.DataMigration
{
    public class DataMigrationPostgreSQLDbContext : DbContext
    {
        public DataMigrationPostgreSQLDbContext(DbContextOptions<DataMigrationPostgreSQLDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var dataMigrationRecordEntity = modelBuilder.Entity<DataMigrationHistoryRecord>();

            dataMigrationRecordEntity.HasKey(_ => _.MigrationId);
        }
    }
}