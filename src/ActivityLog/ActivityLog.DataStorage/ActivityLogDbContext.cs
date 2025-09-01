using ActivityLog.Entities;
using DotNetBrightener.DataAccess.EF.Migrations;
using Microsoft.EntityFrameworkCore;

namespace ActivityLog.DataStorage;

public class ActivityLogDbContext : AdvancedDbContext
{
    protected ActivityLogDbContext(DbContextOptions options)
        : base(options)
    {
    }

    protected ActivityLogDbContext(DbContextOptions options, Action<DbContextOptionsBuilder> optionBuilder)
        : base(options)
    {
    }

    public ActivityLogDbContext(DbContextOptions<ActivityLogDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure ActivityLogRecord entity
        modelBuilder.Entity<ActivityLogRecord>(entity =>
        {
            entity.ToTable(nameof(ActivityLogRecord), nameof(ActivityLog));
            
            entity.HasKey(e => e.Id);

            entity.Property(e => e.ActivityName)
                  .IsRequired()
                  .HasMaxLength(256);

            entity.Property(e => e.ActivityDescription)
                  .HasMaxLength(1000);

            entity.Property(e => e.UserName)
                  .HasMaxLength(256);

            entity.Property(e => e.TargetEntity)
                  .HasMaxLength(256);

            entity.Property(e => e.TargetEntityId)
                  .HasMaxLength(128);

            entity.Property(e => e.StartTime)
                  .IsRequired();

            entity.Property(e => e.MethodName)
                  .HasMaxLength(512);

            entity.Property(e => e.ClassName)
                  .HasMaxLength(256);

            entity.Property(e => e.Namespace)
                  .HasMaxLength(256);

            entity.Property(e => e.ExceptionType)
                  .HasMaxLength(256);

            entity.Property(e => e.Metadata)
                  .HasMaxLength(2048);

            entity.Property(e => e.UserAgent)
                  .HasMaxLength(1024);

            entity.Property(e => e.IpAddress)
                  .HasMaxLength(128);

            entity.Property(e => e.LogLevel)
                  .HasMaxLength(32);

            entity.Property(e => e.Tags)
                  .HasMaxLength(512);

            // Configure indexes for better query performance
            entity.HasIndex(e => e.StartTime)
                  .HasDatabaseName("IX_ActivityLog_StartTime");

            entity.HasIndex(e => e.MethodName)
                  .HasDatabaseName("IX_ActivityLog_MethodName");

            entity.HasIndex(e => e.ClassName)
                  .HasDatabaseName("IX_ActivityLog_ClassName");

            entity.HasIndex(e => e.Namespace)
                  .HasDatabaseName("IX_ActivityLog_Namespace");

            entity.HasIndex(e => e.UserId)
                  .HasDatabaseName("IX_ActivityLog_UserId");

            entity.HasIndex(e => e.CorrelationId)
                  .HasDatabaseName("IX_ActivityLog_CorrelationId");

            entity.HasIndex(e => e.IsSuccess)
                  .HasDatabaseName("IX_ActivityLog_IsSuccess");

            entity.HasIndex(e => e.LogLevel)
                  .HasDatabaseName("IX_ActivityLog_LogLevel");

            // Composite indexes for common query patterns
            entity.HasIndex(e => new { e.StartTime, e.IsSuccess })
                  .HasDatabaseName("IX_ActivityLog_StartTime_IsSuccess");

            entity.HasIndex(e => new { e.Namespace, e.ClassName, e.StartTime })
                  .HasDatabaseName("IX_ActivityLog_Namespace_ClassName_StartTime");
        });
    }
}
