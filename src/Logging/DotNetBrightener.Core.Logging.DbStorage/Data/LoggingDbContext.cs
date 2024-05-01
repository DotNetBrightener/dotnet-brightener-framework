using Microsoft.EntityFrameworkCore;

namespace DotNetBrightener.Core.Logging.DbStorage.Data;

public class LoggingDbContext : DbContext
{
    internal const string SchemaName = "Log";

    public LoggingDbContext(DbContextOptions<LoggingDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var eventLogEntity = modelBuilder.Entity<EventLog>();

        eventLogEntity.ToTable(nameof(EventLog), SchemaName);

        eventLogEntity.HasKey(x => x.Id);
        
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