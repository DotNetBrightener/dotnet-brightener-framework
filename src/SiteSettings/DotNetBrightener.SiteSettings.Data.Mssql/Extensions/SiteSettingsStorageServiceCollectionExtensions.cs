using DotNetBrightener.SiteSettings;
using DotNetBrightener.SiteSettings.Data.Mssql.Data;
using DotNetBrightener.SiteSettings.Data.Mssql.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using System;
using System.Data.Common;
using Microsoft.Data.SqlClient;

// ReSharper disable CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class SiteSettingsStorageServiceCollectionExtensions
{
    extension(IServiceCollection serviceCollection)
    {
        public IServiceCollection AddSiteSettingsSqlServerStorage(string connectionString)
        {
            serviceCollection.AddSiteSettingsSqlServerStorage(((provider, builder) =>
                                                                      new SqlConnection(connectionString)));

            return serviceCollection;
        }

        public IServiceCollection AddSiteSettingsSqlServerStorage(Func<IServiceProvider, DbContextOptionsBuilder,
                                                                          DbConnection>
                                                                      connectionStringResolver)
        {
            serviceCollection.AddDbContext<MssqlStorageSiteSettingDbContext>((serviceProvider, options) =>
            {
                var connection = connectionStringResolver.Invoke(serviceProvider, options);

                options.UseSqlServer(connection,
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
}