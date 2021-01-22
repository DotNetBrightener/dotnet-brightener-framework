using DotNetBrightener.Core.DataAccess.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace DotNetBrightener.Integration.DataMigration.Extensions
{
    public static class AddDataMigrationExtensions
    {
        public static IServiceCollection AddDataMigration(this IServiceCollection serviceCollection,
                                                          DatabaseConfiguration databaseConfiguration,
                                                          IEnumerable<Type> serviceTypes = null)
        {
            serviceCollection.AddScoped<IDataMigrationExecutor, DataMigrationExecutor>();

            if (serviceTypes != null)
            {
                serviceCollection.RegisterServiceImplementations<DataMigrationBase>(serviceTypes,
                                                                                    registerProvidedServiceType: true);
            }
            else
            {
                var appAssemblies = AppDomain.CurrentDomain.GetAssemblies();

                serviceCollection.RegisterServiceImplementations<DataMigrationBase>(appAssemblies,
                                                                                    registerProvidedServiceType: true);
            }

            switch (databaseConfiguration.DatabaseProvider)
            {
                case Core.DataAccess.Abstractions.DatabaseProvider.MsSql:
                    serviceCollection.AddDbContext<DataMigrationMsSQLDbContext>();
                    break;
                case Core.DataAccess.Abstractions.DatabaseProvider.PostgreSql:
                    serviceCollection.AddDbContext<DataMigrationPostgreSQLDbContext>();
                    break;
            }

            return serviceCollection;
        }

        /// <summary>
        ///     Executes the data migration for the current application pipeline
        /// </summary>
        /// <param name="app"></param>
        public static void ExecuteDataMigration(this IApplicationBuilder app)
        {
            using (var scope = app.ApplicationServices.CreateScope())
            {
                var dataMigrationExecutor = scope.ServiceProvider.GetService<IDataMigrationExecutor>();

                dataMigrationExecutor?.UpdateData().Wait();
            }
        }
    }
}