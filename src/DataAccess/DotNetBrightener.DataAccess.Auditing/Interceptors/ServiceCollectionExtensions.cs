using DotNetBrightener.DataAccess.Auditing.Entities;
using DotNetBrightener.DataAccess.EF.Migrations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DotNetBrightener.DataAccess.Auditing.Interceptors;

internal interface IAuditEntriesContainer
{
    List<AuditEntity> AuditEntries { get; }
}

internal class AuditEntriesContainer : IAuditEntriesContainer
{
    public List<AuditEntity> AuditEntries { get; } = new();
}

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAuditContext(this IServiceCollection services)
    {
        services.TryAddScoped<IAuditEntriesContainer, AuditEntriesContainer>();
        services.AddDbContextConfigurator<AuditEnabledDbContextConfigurator>();
        services.AddScoped<AuditEnabledSavingChangesInterceptor>();

        return services;
    }
}

public class AuditEnabledDbContextConfigurator(AuditEnabledSavingChangesInterceptor auditEnabledSavingChangesInterceptor) : IDbContextConfigurator
{
    public void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(auditEnabledSavingChangesInterceptor);
    }
}