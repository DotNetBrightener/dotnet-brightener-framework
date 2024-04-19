using DotNetBrightener.Core.BackgroundTasks.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace DotNetBrightener.Core.BackgroundTasks.Data.DbContexts;

public class BackgroundTaskDbContext : DbContext
{
    public BackgroundTaskDbContext(DbContextOptions<BackgroundTaskDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var eventLogEntity = modelBuilder.Entity<BackgroundTaskDefinition>();

        eventLogEntity.ToTable(nameof(BackgroundTaskDefinition), "BackgroundTask");

        eventLogEntity.HasKey(x => x.Id);

        eventLogEntity.HasIndex(el => el.TaskAssembly)
                      .IncludeProperties(el => el.TaskTypeName);
    }
}