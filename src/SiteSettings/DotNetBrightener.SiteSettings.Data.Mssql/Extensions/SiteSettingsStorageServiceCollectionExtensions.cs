using DotNetBrightener.SiteSettings;
using DotNetBrightener.SiteSettings.Data.Mssql.Data;
using DotNetBrightener.SiteSettings.Data.Mssql.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;

// ReSharper disable CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class SiteSettingsStorageServiceCollectionExtensions
{
    public static IServiceCollection AddSiteSettingsSqlServerStorage(this IServiceCollection serviceCollection,
                                                                     string                  connectionString)
    {
        serviceCollection.AddDbContext<MssqlStorageSiteSettingDbContext>((optionBuilder) =>
        {
            optionBuilder.UseSqlServer(connectionString,
                                       contextOptionsBuilder =>
                                       {
                                           contextOptionsBuilder
                                              .MigrationsHistoryTable("__MigrationsHistory",
                                                                      MssqlStorageSiteSettingDbContext.SchemaName);
                                       })
                         .UseLazyLoadingProxies();
        });

        serviceCollection.AddScoped<ISiteSettingRepository, SiteSettingRepository>();

        serviceCollection.Replace(ServiceDescriptor
                                     .Scoped<ISiteSettingDataService, SqlServerStorageSiteSettingDataService>());
        
        serviceCollection.AddAutoMigrationForDbContextAfterAppStarted<MssqlStorageSiteSettingDbContext>();
        
        return serviceCollection;
    }
}