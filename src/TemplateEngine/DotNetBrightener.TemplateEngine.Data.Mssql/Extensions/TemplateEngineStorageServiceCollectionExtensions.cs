﻿using DotNetBrightener.DataAccess;
using DotNetBrightener.Plugins.EventPubSub;
using DotNetBrightener.TemplateEngine.Data.Mssql.Data;
using DotNetBrightener.TemplateEngine.Data.Mssql.Services;
using DotNetBrightener.TemplateEngine.Data.Services;
using LinqToDB.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;

// ReSharper disable CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class TemplateEngineStorageServiceCollectionExtensions
{
    public static IServiceCollection AddTemplateEngineSqlServerStorage(this IServiceCollection serviceCollection,
                                                                       string                  connectionString)
    {
        serviceCollection.AddDbContext<TemplateEngineDbContext>((optionBuilder) =>
        {
            optionBuilder.UseSqlServer(connectionString,
                                       contextOptionsBuilder =>
                                       {
                                           contextOptionsBuilder
                                              .MigrationsHistoryTable("__MigrationsHistory",
                                                                      TemplateEngineDbContext.SchemaName);
                                       })
                         .UseLazyLoadingProxies();
        });

        serviceCollection.AddScoped<TemplateEngineRepository>();

        serviceCollection.Replace(ServiceDescriptor.Scoped<ITemplateRegistrationService, SqlServerTemplateRegistrationService>());
        serviceCollection.Replace(ServiceDescriptor.Scoped<ITemplateStorageService, SqlServerTemplateStorageService>());
        serviceCollection.AddScoped<ITemplateRecordDataService, InternalTemplateRecordDataService>();

        serviceCollection.AddAutoMigrationForDbContextAfterAppStarted<TemplateEngineDbContext>();

        LinqToDBForEFTools.Initialize();

        return serviceCollection;
    }
}