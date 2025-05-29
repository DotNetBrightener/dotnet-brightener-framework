using DotNetBrightener.SiteSettings;
using DotNetBrightener.SiteSettings.Data.PostgreSql.Data;
using DotNetBrightener.SiteSettings.Data.PostgreSql.Extensions;
using LinqToDB.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;

// ReSharper disable CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class SiteSettingsStorageServiceCollectionExtensions
{
    public static IServiceCollection AddSiteSettingsPostgreSqlStorage(this IServiceCollection serviceCollection,
                                                                      string                  connectionString)
    {
        serviceCollection.AddDbContext<PostgreSqlStorageSiteSettingDbContext>((optionBuilder) =>
        {
            optionBuilder.UseNpgsql(connectionString,
                                    contextOptionsBuilder =>
                                    {
                                        contextOptionsBuilder
                                           .MigrationsHistoryTable("__MigrationsHistory",
                                                                   PostgreSqlStorageSiteSettingDbContext.SchemaName);
                                    })
                         .UseLazyLoadingProxies();
        });

        serviceCollection.AddScoped<ISiteSettingRepository, SiteSettingRepository>();

        serviceCollection.Replace(ServiceDescriptor
                                     .Scoped<ISiteSettingDataService, PostgreSqlStorageSiteSettingDataService>());

        serviceCollection.AddAutoMigrationForDbContextAfterAppStarted<PostgreSqlStorageSiteSettingDbContext>();

        LinqToDBForEFTools.Initialize();

        return serviceCollection;
    }
}