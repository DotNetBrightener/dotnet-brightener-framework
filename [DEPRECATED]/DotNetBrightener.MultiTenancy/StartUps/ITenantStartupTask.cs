using System.Threading.Tasks;

namespace DotNetBrightener.MultiTenancy.StartUps
{
    /// <summary>
    ///     Declares the task that will be executed at start up of the tenant
    /// </summary>
    public interface ITenantStartupTask
    {
        /// <summary>
        ///     The priority of the task.
        /// </summary>
        int Order { get; }

        /// <summary>
        ///     Executes the task
        /// </summary>
        Task Execute();
    }

    /// <summary>
    ///     Just a default task which does nothing
    /// </summary>
    internal class DefaultTenantStartupTask : ITenantStartupTask
    {
        public int Order => 0;

        public Task Execute()
        {
            return Task.CompletedTask;
        }
    }
}