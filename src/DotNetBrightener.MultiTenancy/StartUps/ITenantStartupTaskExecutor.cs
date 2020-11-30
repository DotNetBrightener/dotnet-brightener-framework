using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace DotNetBrightener.MultiTenancy.StartUps
{
    /// <summary>
    ///     The service which executes all the tasks at the start up of each tenant
    /// </summary>
    public interface ITenantStartupTaskExecutor
    {
        /// <summary>
        ///     Executes all the available tasks
        /// </summary>
        Task ExecuteAll();
    }

    internal class DefaultTenantStartupTaskExecutor : ITenantStartupTaskExecutor
    {
        private readonly IEnumerable<ITenantStartupTask> _startupTasks;
        private readonly ILogger                         _logger;

        public DefaultTenantStartupTaskExecutor(IEnumerable<ITenantStartupTask>           startupTasks,
                                                ILogger<DefaultTenantStartupTaskExecutor> logger)
        {
            _logger = logger;
            _startupTasks = startupTasks.Where(_ => _.GetType() != typeof(DefaultTenantStartupTask))
                                        .OrderBy(_ => _.Order);
        }

        public async Task ExecuteAll()
        {
            var allTasks = _startupTasks.Select(ExecuteTask);

            await Task.WhenAll(allTasks);
        }

        private Task ExecuteTask(ITenantStartupTask tenantStartupTask)
        {
            try
            {
                return tenantStartupTask.Execute();
            }
            catch (Exception exception)
            {
                _logger.LogWarning(exception,
                                   $"Error while executing startup task {tenantStartupTask.GetType()}");

                return Task.CompletedTask;
            }
        }
    }
}