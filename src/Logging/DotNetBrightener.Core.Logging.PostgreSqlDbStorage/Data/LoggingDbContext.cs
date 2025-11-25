using DotNetBrightener.Core.Logging.PostgreSqlDbStorage.Internal;
using Microsoft.EntityFrameworkCore;

namespace DotNetBrightener.Core.Logging.PostgreSqlDbStorage.Data;

public class LoggingDbContext(DbContextOptions<LoggingDbContext> options) : DbContext(options)
{
    internal const string SchemaName = "Log";

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);

        configurationBuilder.Properties<DateTimeOffset>()
                            .HaveConversion<DateTimeOffsetNpgsqlConverter>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var eventLogEntity = modelBuilder.Entity<EventLog>();

        eventLogEntity.ToTable(nameof(EventLog), SchemaName);

        eventLogEntity.HasKey(x => x.Id);

        eventLogEntity.Property(e => e.Id)
                      .ValueGeneratedNever();

        // single indexes
        eventLogEntity.HasIndex(el => el.Level);
        eventLogEntity.HasIndex(el => el.TimeStamp);
        eventLogEntity.HasIndex(el => el.LoggerName);

        // composite indexes
        eventLogEntity.HasIndex(el => el.TimeStamp)
                      .IncludeProperties(el => el.Level);
        
        eventLogEntity.HasIndex(el => el.TimeStamp)
                      .IncludeProperties(el => new
                       {
                           el.Level,
                           el.LoggerName
                      });

        eventLogEntity.HasIndex(el => el.LoggerName);
        eventLogEntity.HasIndex(el => el.LoggerName)
                      .IncludeProperties(el => new
                       {
                           el.Level,
                           el.TimeStamp
                      });

        eventLogEntity.HasIndex(el => el.RequestId)
                      .IncludeProperties(el => new
                       {
                           el.TimeStamp,
                           el.Level
                       });
    }
}