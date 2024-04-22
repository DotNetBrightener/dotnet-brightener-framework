using DotNetBrightener.Infrastructure.AppClientManager.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotNetBrightener.Infrastructure.AppClientManager.DataStorage.EntityTypeConfigurations;

public class AppClientAccessScopeEntityTypeConfiguration : IEntityTypeConfiguration<AppClientAccessScope>
{
    public void Configure(EntityTypeBuilder<AppClientAccessScope> builder)
    {
        builder.ToTable(nameof(AppClientAccessScope), schema: AppClientDataDefaults.AppClientSchemaName);
    }
}