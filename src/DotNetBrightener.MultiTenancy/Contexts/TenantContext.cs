using System;
using System.Threading;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DotNetBrightener.MultiTenancy.Contexts
{
    /// <summary>
    ///		Represents the whole context for the current request which is adapted for particular tenant
    /// </summary>
    public class TenantContext : IDisposable
    {
        private volatile int     _refCount = 0;
        private readonly ILogger _logger;
        private          bool    _disposed = false;
        private          bool    _released = false;

        /// <summary>
        ///		The associated <see cref="ITenant"/>
        /// </summary>
        public ITenant Tenant { get; set; }

        /// <summary>
        ///		The <see cref="IServiceProvider" /> to resolve dependencies and services for the foreground operations
        /// </summary>
        public IServiceProvider MainServiceProvider { get; private set; }
        
        /// <summary>
        ///		Indicates if the tenant is activated
        /// </summary>
        public bool IsActivated { get; set; }

        /// <summary>
        ///		Indicates if the tenant needs to be restarted
        /// </summary>
        public bool NeedRestart { get; set; }

        public TenantContext(ITenant         tenant,
                             TenantContainer tenantContainer)
        {
            Tenant                  = tenant;
            MainServiceProvider       = tenantContainer.ForegroundServiceProvider;
            _logger                   = MainServiceProvider.GetService<ILogger<TenantContext>>();
        }

        public IServiceScope EnterServiceScope()
        {
            if (_disposed)
            {
                throw new InvalidOperationException("Can't use EnterServiceScope on a disposed context");
            }

            if (_released)
            {
                throw new InvalidOperationException("Can't use EnterServiceScope on a released context");
            }

            return new ServiceScopeWrapper(this);
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            var tenantName = Tenant.Name;

            _logger.LogInformation($"Disposing Context for tenant ${tenantName}");

            // Disposes all the services registered for this shell
            if (MainServiceProvider != null)
            {
                (MainServiceProvider as IDisposable)?.Dispose();
                MainServiceProvider = null;
            }

            IsActivated = false;
            Tenant    = null;

            _disposed = true;

            _logger.LogInformation($"Disposed Context for tenant ${tenantName}");

            GC.SuppressFinalize(this);
        }

        ~TenantContext()
        {
            Dispose();
        }

        internal class ServiceScopeWrapper : IServiceScope
        {
            private readonly TenantContext    _tenantContext;
            private readonly HttpContext      _httpContext;
            private readonly IServiceScope    _serviceScope;
            private readonly IServiceProvider _existingServices;

            public ServiceScopeWrapper(TenantContext tenantContext)
            {
                // Prevent the context from being released until the end of the scope
                Interlocked.Increment(ref tenantContext._refCount);

                _tenantContext  = tenantContext;
                _serviceScope   = tenantContext.MainServiceProvider.CreateScope();
                ServiceProvider = _serviceScope.ServiceProvider;

                var httpContextAccessor = ServiceProvider.GetRequiredService<IHttpContextAccessor>();

                if (httpContextAccessor.HttpContext == null)
                {
                    httpContextAccessor.HttpContext = new DefaultHttpContext();
                }

                _httpContext                 = httpContextAccessor.HttpContext;
                _existingServices            = _httpContext.RequestServices;
                _httpContext.RequestServices = ServiceProvider;
            }

            public IServiceProvider ServiceProvider { get; }

            /// <summary>
            /// Returns true is the shell context should be disposed consequently to this scope being released.
            /// </summary>
            private bool ScopeReleased()
            {
                var refCount = Interlocked.Decrement(ref _tenantContext._refCount);

                return _tenantContext._released && refCount == 0;
            }

            public void Dispose()
            {
                var disposeShellContext = ScopeReleased();

                try
                {
                    _httpContext.RequestServices = _existingServices;
                }
                catch (ObjectDisposedException)
                {
                    // ignore exception, it's because the httpcontext got released
                }

                _serviceScope.Dispose();

                GC.SuppressFinalize(this);

                if (disposeShellContext)
                {
                    _tenantContext.Dispose();
                }
            }

            ~ServiceScopeWrapper()
            {
                Dispose();
            }
        }

        public void Release()
        {
            if (_released == true)
            {
                // Prevent infinite loops with circular dependencies
                return;
            }

            _released = true;

            lock (this)
            {
                if (_refCount == 0)
                {
                    Dispose();
                }
            }
        }
    }
}