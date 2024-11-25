using DotNetBrightener.DataAccess.EF.Migrations;
using Microsoft.EntityFrameworkCore;

namespace DotNetBrightener.DataAccess.EF.Interceptors;

internal class AuditInformationFillerDbContextConfigurator(AuditInformationFillerInterceptor auditInformationFillerInterceptor)
    : IDbContextConfigurator
{
    public void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(auditInformationFillerInterceptor);
    }
}