using DotNetBrightener.DataAccess.Auditing.Interceptors;
using DotNetBrightener.DataAccess.Auditing.Internal;
using DotNetBrightener.DataAccess.EF.Migrations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;

// ReSharper disable CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

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