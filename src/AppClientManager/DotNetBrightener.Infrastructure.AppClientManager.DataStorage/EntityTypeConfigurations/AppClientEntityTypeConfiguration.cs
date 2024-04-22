using DotNetBrightener.Infrastructure.AppClientManager.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotNetBrightener.Infrastructure.AppClientManager.DataStorage.EntityTypeConfigurations;

public class AppClientEntityTypeConfiguration : IEntityTypeConfiguration<AppClient>
{
    public void Configure(EntityTypeBuilder<AppClient> builder)
    {
        builder.ToTable(nameof(AppClient), schema: AppClientDataDefaults.AppClientSchemaName);

        builder.HasIndex(_ => _.ClientId)
               .IsUnique();

        builder.HasIndex(_ => _.AllowedOrigins);
    }
}