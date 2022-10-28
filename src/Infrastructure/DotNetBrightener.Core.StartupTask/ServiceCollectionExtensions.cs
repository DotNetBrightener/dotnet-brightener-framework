// ReSharper disable CheckNamespace
using System;
using System.Linq;
using System.Threading.Tasks;
using DotNetBrightener.Core.StartupTask;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static void RegisterStartupTask<T>(this IServiceCollection serviceCollection)
        where T : class, IStartupTask
    {
        serviceCollection.AddScoped<IStartupTask, T>();
    }

    public static async Task ExecuteStartupTasks(this IServiceProvider serviceProvider)
    {
        using (var serviceScope = serviceProvider.CreateScope())
        {
            var startupTasks = serviceScope.ServiceProvider.GetServices<IStartupTask>()
                                           .OrderBy(_ => _.Order);

            foreach (var startupTask in startupTasks)
            {
                await startupTask.Execute();
            }
        }
    }
}
