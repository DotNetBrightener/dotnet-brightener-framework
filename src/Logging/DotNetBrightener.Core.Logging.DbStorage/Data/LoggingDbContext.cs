using Microsoft.EntityFrameworkCore;

namespace DotNetBrightener.Core.Logging.DbStorage.Data;

public class LoggingDbContext : DbContext
{
    public LoggingDbContext(DbContextOptions<LoggingDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var eventLogEntity = modelBuilder.Entity<EventLog>();

        eventLogEntity.ToTable(nameof(EventLog), "Logging");

        eventLogEntity.HasKey(x => x.Id);

        eventLogEntity.Property(el => el.LoggerName)
                      .HasMaxLength(1024);

        eventLogEntity.Property(el => el.Level)
                      .HasMaxLength(32);

        eventLogEntity.Property(el => el.RemoteIpAddress)
                      .HasMaxLength(64);

        eventLogEntity.Property(el => el.RequestId)
                      .HasMaxLength(512);

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