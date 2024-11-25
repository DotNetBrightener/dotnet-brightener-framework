using DotNetBrightener.DataAccess.EF.Interceptors;
using DotNetBrightener.DataAccess.EF.Migrations;
using Microsoft.EntityFrameworkCore;

namespace DotNetBrightener.DataAccess.EF.Auditing;

internal class AuditEnabledDbContextConfigurator(AuditEnabledSavingChangesInterceptor auditEnabledSavingChangesInterceptor) 
    : IDbContextConfigurator
{
    public void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(auditEnabledSavingChangesInterceptor);
    }
}