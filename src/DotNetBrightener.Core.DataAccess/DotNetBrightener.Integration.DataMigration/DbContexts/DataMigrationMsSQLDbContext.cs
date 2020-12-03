using Microsoft.EntityFrameworkCore;

namespace DotNetBrightener.Integration.DataMigration
{
    public class DataMigrationMsSQLDbContext : DbContext
    {
        public DataMigrationMsSQLDbContext(DbContextOptions<DataMigrationMsSQLDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var dataMigrationRecordEntity = modelBuilder.Entity<DataMigrationHistoryRecord>();

            dataMigrationRecordEntity.HasKey(_ => _.MigrationId);
        }
    }
}