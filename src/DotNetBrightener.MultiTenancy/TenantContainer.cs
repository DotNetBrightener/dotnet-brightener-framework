using System;

namespace DotNetBrightener.MultiTenancy
{
    public class TenantContainer
    {
        public ITenant Tenant { get; }

        /// <summary>
        ///     The service provider that host the background tasks
        /// </summary>
        public IServiceProvider BackgroundServiceProvider { get; internal set; }

        /// <summary>
        ///     The service provider that host the foreground tasks e.g processing HTTP requests
        /// </summary>
        public IServiceProvider ForegroundServiceProvider { get; internal set; }

        public TenantContainer(ITenant tenant)
        {
            Tenant = tenant;
        }
    }
}