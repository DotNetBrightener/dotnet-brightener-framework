using DotNetBrightener.Core.BackgroundTasks.Data.DbContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DotNetBrightener.Core.BackgroundTasks.Data;

internal class MigrateBackgroundTaskDbContextHostedService : IHostedService, IDisposable
{
    private readonly IServiceScopeFactory                                 _serviceScopeFactory;
    private readonly ILogger<MigrateBackgroundTaskDbContextHostedService> _logger;
    private readonly IHostApplicationLifetime                             _lifetime;

    public MigrateBackgroundTaskDbContextHostedService(IServiceScopeFactory serviceScopeFactory,
                                                       ILogger<MigrateBackgroundTaskDbContextHostedService> logger,
                                                       IHostApplicationLifetime lifetime)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger              = logger;
        _lifetime            = lifetime;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _lifetime.ApplicationStarted.Register(InitializeAfterAppStarted);

        return Task.CompletedTask;
    }

    private void InitializeAfterAppStarted()
    {
        using var scope     = _serviceScopeFactory.CreateScope();
        using var dbContext = scope.ServiceProvider.GetRequiredService<BackgroundTaskDbContext>();

        var pendingMigrations = dbContext.Database
                                         .GetPendingMigrations()
                                         .ToArray();

        if (!pendingMigrations.Any())
            return;

        _logger.LogInformation("Migrating database for {dbContextName}", dbContext.GetType().Name);

        dbContext.Database.Migrate();

        _logger.LogInformation("Database migration completed for {dbContextName}", dbContext.GetType().Name);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
    }

    public void Dispose()
    {
    }
}