using DotNetBrightener.DataAccess.EF.Migrations;
using Microsoft.EntityFrameworkCore;

namespace DotNetBrightener.DataAccess.Auditing.Interceptors;

internal class AuditEnabledDbContextConfigurator(AuditEnabledSavingChangesInterceptor auditEnabledSavingChangesInterceptor) : IDbContextConfigurator
{
    public void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(auditEnabledSavingChangesInterceptor);
    }
}