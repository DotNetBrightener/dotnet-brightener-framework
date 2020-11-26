using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using DotNetBrightener.Integration.GraphQL.Extensions;
using DotNetBrightener.Integration.GraphQL.Transactions;
using GraphQL;
using GraphQL.Execution;
using GraphQL.Server;
using GraphQL.Server.Ui.Playground;
using GraphQL.Types;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetBrightener.Integration.GraphQL.Extensions
{
    public static class GraphQlEnableServiceCollectionExtensions
    {
        private static readonly Dictionary<IServiceCollection, IGraphQLBuilder> GraphQlBuildersCache =
            new Dictionary<IServiceCollection, IGraphQLBuilder>();

        public static IServiceCollection EnableGraphQL(this IServiceCollection serviceCollection)
        {
            if (GraphQlBuildersCache.TryGetValue(serviceCollection, out var builder))
            {
                throw new
                    InvalidOperationException($"An IGraphBuilder has been registered for the given IServiceCollection.");
            }

            serviceCollection.EnableDataTransaction();

            void ConfigureErrorInfoProvider(ErrorInfoProviderOptions options, IServiceProvider provider)
            {
                var environment = provider.GetService<IWebHostEnvironment>();
                options.ExposeExtensions = false;
                //options.ExposeExceptionStackTrace = environment.IsDevelopment();
            }

            builder = serviceCollection.AddGraphQL(option =>
            {
                option.UnhandledExceptionDelegate = (context) =>
                {
                    context.Context.ThrowOnUnhandledException = true;
                };
            })
                                       .AddSystemTextJson()
                                       .AddDataLoader()
                                       .AddErrorInfoProvider(ConfigureErrorInfoProvider);

            GraphQlBuildersCache.Add(serviceCollection, builder);
            serviceCollection.AddScoped<DnbAppSchema>();
            serviceCollection.AddScoped<ISchema, DnbAppSchema>();
            serviceCollection.AddGraphTypes();

            ValueConverter.Register(
                                    typeof(float),
                                    typeof(double),
                                    value =>
                                        Convert.ToDouble(Math.Round((float) value, 3, MidpointRounding.AwayFromZero),
                                                         NumberFormatInfo.InvariantInfo));

            ValueConverter.Register(
                                    typeof(double),
                                    typeof(float),
                                    value => Convert.ToSingle(value));

            return serviceCollection;
        }

        public static IServiceCollection AddGraphTypes(this IServiceCollection serviceCollection,
                                                       ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
        {
            if (!GraphQlBuildersCache.TryGetValue(serviceCollection, out var builder))
            {
                throw new
                    InvalidOperationException($"IGraphBuilder has not been registered to the given IServiceCollection. Make sure you call {nameof(EnableGraphQL)}() before registering GraphTypes");
            }

            builder.AddGraphTypes(Assembly.GetCallingAssembly(), serviceLifetime);

            return serviceCollection;
        }

        public static IServiceCollection AddGraphTypes(this IServiceCollection serviceCollection,
                                                       Assembly assembly,
                                                       ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
        {
            if (!GraphQlBuildersCache.TryGetValue(serviceCollection, out var builder))
            {
                throw new
                    InvalidOperationException($"IGraphBuilder has not been registered to the given IServiceCollection. Make sure you call {nameof(EnableGraphQL)}() before registering GraphTypes");
            }

            builder.AddGraphTypes(assembly, serviceLifetime);

            return serviceCollection;
        }

        public static void UseGraphQLIntegration(this IApplicationBuilder appBuilder, string path = null)
        {
            if (string.IsNullOrEmpty(path))
                appBuilder.UseGraphQL<DnbAppSchema>();
            else
                appBuilder.UseGraphQL<DnbAppSchema>(path);

            appBuilder.UseGraphQLPlayground(options: new GraphQLPlaygroundOptions());
        }
    }
}