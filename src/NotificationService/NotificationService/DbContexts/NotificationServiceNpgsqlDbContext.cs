using DotNetBrightener.DataAccess.EF.Migrations;
using Microsoft.EntityFrameworkCore;
using NotificationService.Entity;

namespace NotificationService.DbContexts;

internal abstract class NotificationServiceBaseDbContext(DbContextOptions options)
    : AdvancedDbContext(options)
{
    public const string SchemaName = "NotificationService";

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<NotificationMessageQueue>(builder =>
        {
            builder.ToTable(nameof(NotificationMessageQueue), schema: SchemaName);
        });
    }
}