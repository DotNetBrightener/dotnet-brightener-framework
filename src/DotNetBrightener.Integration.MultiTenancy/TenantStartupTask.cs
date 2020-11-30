using System.Threading.Tasks;
using DotNetBrightener.Core.StartupTask;
using DotNetBrightener.MultiTenancy;
using DotNetBrightener.MultiTenancy.StartUps;

namespace DotNetBrightener.Integration.MultiTenancy
{
    public class TenantStartupTask : ITenantStartupTask
    {
        private readonly ITenant             _tenant;
        private readonly IStartupTasksRunner _startupTasksRunner;

        public TenantStartupTask(ITenant             tenant,
                                 IStartupTasksRunner startupTasksRunner)
        {
            _tenant             = tenant;
            _startupTasksRunner = startupTasksRunner;
        }

        public int Order => 10;

        public Task Execute()
        {
            if (!_tenant.IsActive)
                return Task.CompletedTask;

            return _startupTasksRunner.Execute();
        }
    }
}