// ReSharper disable CheckNamespace
using DotNetBrightener.Core.StartupTask;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     Registers a service that is executed at the application startup to the service collection
    /// </summary>
    /// <typeparam name="T">Type of the startup task service, derived from <see cref="IStartupTask"/></typeparam>
    /// <param name="serviceCollection">
    ///     The <see cref="IServiceCollection"/>
    /// </param>
    public static void RegisterStartupTask<T>(this IServiceCollection serviceCollection)
        where T : class, IStartupTask
    {
        serviceCollection.AddScoped<IStartupTask, T>();
    }

    /// <summary>
    ///     Executes the startup tasks from the given service provider
    /// </summary>
    /// <param name="serviceProvider">
    ///     The <see cref="IServiceProvider"/> to extract the startup tasks and execute them
    /// </param>
    /// <returns></returns>
    public static async Task ExecuteStartupTasks(this IServiceProvider serviceProvider)
    {
        using (var serviceScope = serviceProvider.CreateScope())
        {
            var startupTasks = serviceScope.ServiceProvider
                                           .GetServices<IStartupTask>()
                                           .OrderBy(_ => _.Order);

            foreach (var startupTask in startupTasks)
            {
                await startupTask.Execute();
            }
        }
    }
}
