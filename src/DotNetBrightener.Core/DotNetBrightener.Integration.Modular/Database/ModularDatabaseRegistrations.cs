using System;
using System.Collections.Generic;
using System.Linq;
using DotNetBrightener.Core.DataAccess;
using DotNetBrightener.Core.DataAccess.EF.Extensions;
using DotNetBrightener.Core.DataAccess.EF.Migrations.Schema;
using DotNetBrightener.Core.DataAccess.EF.Repositories;
using DotNetBrightener.Integration.Modular.Database.Migration;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DotNetBrightener.Integration.Modular.Database
{
    public static class ModularDatabaseRegistrations
    {
        /// <summary>
        ///     Re-configure the <see cref="DbContext"/> service registrations in the <see cref="IServiceCollection"/>
        ///     in the modular approach to share the same connection string and database provider settings
        /// </summary>
        /// <param name="serviceCollection"></param>
        /// <param name="databaseConfiguration">The <see cref="DatabaseConfiguration"/></param>
        public static void ConfigureModularDbContexts<TCentralizedDbContext>(this IServiceCollection serviceCollection,
                                                                             DatabaseConfiguration databaseConfiguration)
            where TCentralizedDbContext : DotNetBrightenerDbContext
        {
            var dbOptionTypes =
                serviceCollection.Where(_ => typeof(DbContextOptions).IsAssignableFrom(_.ServiceType))
                                 .ToArray();

            foreach (var serviceDescriptor in dbOptionTypes)
            {
                serviceCollection.Remove(serviceDescriptor);
            }

            var dbContextTypes =
                serviceCollection.Where(_ => typeof(DbContext).IsAssignableFrom(_.ServiceType))
                                 .ToArray();

            var dbContextsToRegister = new List<Type>();

            foreach (var serviceDescriptor in dbContextTypes)
            {
                dbContextsToRegister.Add(serviceDescriptor.ServiceType);
                serviceCollection.Remove(serviceDescriptor);
            }

            foreach (var dbContext in dbContextsToRegister)
            {
                serviceCollection.RegisterDbContext(dbContext, databaseConfiguration);
            }

            serviceCollection.RegisterDbContext<TCentralizedDbContext, DotNetBrightenerDbContext>(databaseConfiguration);
            serviceCollection.Replace(ServiceDescriptor
                                         .Scoped<ISchemaMigrationExecutor, ModularDbSchemaMigrationExecutor>());
        }

        public static void AutoMigrateModuleDatabases(this IApplicationBuilder app)
        {
            using (var scope = app.ApplicationServices.CreateScope())
            {
                var schemaMigrationExecutor = scope.ServiceProvider.GetService<ISchemaMigrationExecutor>();

                schemaMigrationExecutor?.MigrateDatabase().Wait();
            }
        }
    }
}