using DotNetBrightener.Core.BackgroundTasks.Data.DbContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DotNetBrightener.Core.BackgroundTasks.Data;

internal class MigrateBackgroundTaskDbContextHostedService(
    IServiceScopeFactory                                 serviceScopeFactory,
    ILogger<MigrateBackgroundTaskDbContextHostedService> logger,
    IHostApplicationLifetime                             lifetime)
    : IHostedService, IDisposable
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        lifetime.ApplicationStarted.Register(InitializeAfterAppStarted);

        return Task.CompletedTask;
    }

    private void InitializeAfterAppStarted()
    {
        using var scope     = serviceScopeFactory.CreateScope();
        using var dbContext = scope.ServiceProvider.GetRequiredService<BackgroundTaskDbContext>();

        var pendingMigrations = dbContext.Database
                                         .GetPendingMigrations()
                                         .ToArray();

        if (!pendingMigrations.Any())
            return;

        logger.LogInformation("Migrating database for {dbContextName}", dbContext.GetType().Name);

        dbContext.Database.Migrate();

        logger.LogInformation("Database migration completed for {dbContextName}", dbContext.GetType().Name);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
    }

    public void Dispose()
    {
    }
}