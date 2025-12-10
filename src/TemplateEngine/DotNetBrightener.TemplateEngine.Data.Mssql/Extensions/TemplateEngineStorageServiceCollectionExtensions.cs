using DotNetBrightener.DataAccess;
using DotNetBrightener.TemplateEngine.Data.Mssql.Data;
using DotNetBrightener.TemplateEngine.Data.Mssql.Services;
using DotNetBrightener.TemplateEngine.Data.Services;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Data.Common;

// ReSharper disable CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class TemplateEngineStorageServiceCollectionExtensions
{
    extension(IServiceCollection serviceCollection)
    {
        public IServiceCollection AddTemplateEngineSqlServerStorage(string connectionString)
        {
            return serviceCollection.AddTemplateEngineSqlServerStorage((provider, builder) =>
                                                                           new SqlConnection(connectionString));
        }

        public IServiceCollection AddTemplateEngineSqlServerStorage(Func<IServiceProvider, DbContextOptionsBuilder,
                                                                            DbConnection>
                                                                        connectionStringResolver)
        {
            serviceCollection.AddDbContext<TemplateEngineDbContext>((serviceProvider, optionBuilder) =>
            {
                var connection = connectionStringResolver.Invoke(serviceProvider, optionBuilder);

                optionBuilder.UseSqlServer(connection,
                                           contextOptionsBuilder =>
                                           {
                                               contextOptionsBuilder
                                                  .MigrationsHistoryTable("__MigrationsHistory",
                                                                          TemplateEngineDbContext.SchemaName);
                                           })
                             .UseLazyLoadingProxies();
            });

            serviceCollection.TryAddScoped<ScopedCurrentUserResolver>();

            serviceCollection.Replace(ServiceDescriptor
                                         .Scoped<ITemplateRegistrationService, SqlServerTemplateRegistrationService>());
            serviceCollection.Replace(ServiceDescriptor
                                         .Scoped<ITemplateStorageService, SqlServerTemplateStorageService>());

            serviceCollection.AddScoped<TemplateEngineRepository>();
            serviceCollection.AddScoped<ITemplateRecordDataService, InternalTemplateRecordDataService>();

            return serviceCollection;
        }
    }
}