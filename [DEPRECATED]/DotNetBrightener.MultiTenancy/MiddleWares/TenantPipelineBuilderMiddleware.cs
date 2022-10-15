using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DotNetBrightener.MultiTenancy.Contexts;
using DotNetBrightener.MultiTenancy.Exceptions;
using DotNetBrightener.MultiTenancy.Services;
using DotNetBrightener.MultiTenancy.StartUps;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DotNetBrightener.MultiTenancy.MiddleWares
{
    public class TenantPipelineBuilderMiddleware
    {
        private          bool                            _allContextsReady;
        private readonly MultiTenantConfigurationBuilder _configurationBuilder;
        private readonly RequestDelegate                 _next;
        private readonly ILogger                         _logger;

        private readonly ConcurrentDictionary<string, SemaphoreSlim> _semaphores =
            new ConcurrentDictionary<string, SemaphoreSlim>();

        private const string ContextInitializingSemaphoreKey = "[CONTEXT_INITIALIZING]";

        public TenantPipelineBuilderMiddleware(RequestDelegate                          next,
                                               MultiTenantConfigurationBuilder          configurationBuilder,
                                               ILogger<TenantPipelineBuilderMiddleware> logger)
        {
            _next                 = next;
            _logger               = logger;
            _configurationBuilder = configurationBuilder;
        }

        public async Task Invoke(HttpContext              httpContext,
                                 IApplicationHost         applicationHost,
                                 IRunningTenantTable      runningTenantTable,
                                 ITenantPipelineContainer tenantPipelineContainer)
        {
            // Ensure all TenantContexts are loaded and available.
            applicationHost.Initialize();

            // build all tenant contexts before they are hit by first request.
            var tenantContexts = applicationHost.ListTenantContexts();

            // initialize all tenants for the first time
            if (!_allContextsReady)
            {
                var semaphore = _semaphores.GetOrAdd(ContextInitializingSemaphoreKey, (name) => new SemaphoreSlim(1));
                // a kind of locking mechanism, making sure all contexts are built within only 1 request.
                // When the second request comes, it will wait
                await semaphore.WaitAsync();

                // check again here to make sure all contexts are built
                if (!_allContextsReady)
                {
                    Debugger.Launch();

                    Parallel.ForEach(tenantContexts,
                                     async tenantContext =>
                                     {
                                         _logger
                                            .LogInformation($"[SYSTEM] - Started building pipeline for tenant {tenantContext.Tenant.Name}");

                                         await RebuildTenantPipelineIfNeeded(tenantContext, tenantPipelineContainer);

                                         _logger
                                            .LogInformation($"[SYSTEM] - Finished building pipeline for tenant {tenantContext.Tenant.Name}");
                                     });

                    _allContextsReady = true;
                }

                // release the lock
                semaphore.Release();
                _semaphores.TryRemove(ContextInitializingSemaphoreKey, out _);
            }

            if (httpContext == null)
            {
                _logger.LogInformation($"No HttpContext available. Cancelling request.");
                return;
            }

            // detect current tenant and see if it needs restart
            var tenantSetting = runningTenantTable.DetectTenantFromContext(httpContext);

            if (tenantSetting == null)
                throw new TenantCannotIdentifyException();

            _logger.LogInformation($"[{tenantSetting.Name}] - Taking over request handling");
            var currentTenantContext = applicationHost.GetOrCreateTenantContext(tenantSetting);

            // if the tenant shell context is marked to be restarted or is not activated, activate it here
            await RebuildTenantPipelineIfNeeded(currentTenantContext, tenantPipelineContainer);

            httpContext.Features.Set(currentTenantContext);
            httpContext.Features.Set(tenantSetting);

            httpContext.Items["CurrentTenantId"]      = tenantSetting.Id;
            httpContext.Items["CurrentTenant"]        = tenantSetting;
            httpContext.Items["CurrentTenantContext"] = currentTenantContext;

            await _next.Invoke(httpContext);
        }

        private async Task RebuildTenantPipelineIfNeeded(TenantContext            tenantContext,
                                                         ITenantPipelineContainer tenantPipelineContainer,
                                                         bool                     forceRebuild = false)
        {
            // a tenant needs to be rebuild if it is told to be, or is marked as need restart, or is not activated
            var needRebuild = forceRebuild || tenantContext.NeedRestart || !tenantContext.IsActivated;
            var tenantName  = tenantContext.Tenant.Name;

            // if need rebuilding, remove the tenant pipeline from the container
            if (needRebuild && tenantPipelineContainer.ContainsPipelineForTenant(tenantName))
            {
                _logger.LogInformation($"[{tenantName}] - Need re-initializing");

                tenantPipelineContainer.RemovePipeline(tenantName);
            }

            // if the pipeline has not been initialized, build it here
            // in case of rebuilding, the pipeline got removed from container, so it will be rebuilt here too
            if (!tenantPipelineContainer.ContainsPipelineForTenant(tenantName))
            {
                var semaphore = _semaphores.GetOrAdd(tenantContext.Tenant.Name, (name) => new SemaphoreSlim(1));
                // a kind of locking mechanism, to wait for the current tenant context to finish building
                await semaphore.WaitAsync();

                try
                {
                    // check again to make sure the pipeline is not added to the container
                    if (!tenantPipelineContainer.ContainsPipelineForTenant(tenantName))
                    {
                        _logger.LogInformation($"[{tenantName}] - Building pipeline");

                        var pipeline = BuildTenantPipeline(tenantContext.MainServiceProvider);
                        tenantPipelineContainer.AddPipeline(tenantName, pipeline);

                        // mark the tenant context as activated and no need to restart
                        tenantContext.IsActivated = true;
                        tenantContext.NeedRestart = false;

                        _logger.LogInformation($"[{tenantName}] - Pipeline build finished");
                    }
                }
                finally
                {
                    // release the lock
                    semaphore.Release();
                    _semaphores.TryRemove(tenantContext.Tenant.Name, out _);
                }
            }
        }

        /// <summary>
        ///     Builds the middleware pipeline for the current tenant
        /// </summary>
        /// <param name="tenantServiceProvider"></param>
        /// <returns></returns>
        public RequestDelegate BuildTenantPipeline(IServiceProvider tenantServiceProvider)
        {
            // create an application builder that will delegate to the given tenant
            var tenantScopedApplicationBuilder = new ApplicationBuilder(tenantServiceProvider);

            Action<IApplicationBuilder> configure = ConfigureTenantPipeline;

            // Create a nested pipeline to configure the tenant middleware pipeline
            var startupFilters = tenantScopedApplicationBuilder
                                .ApplicationServices.GetService<IEnumerable<IStartupFilter>>();

            // performs the nested configuration from framework (eg. making a chain of Configure() calls)
            foreach (var filter in startupFilters.Reverse())
            {
                configure = filter.Configure(configure);
            }

            // triggering configure the tenant, which invokes the Configure() methods of each StartUp class
            // to build up the app container for the tenant
            configure?.Invoke(tenantScopedApplicationBuilder);

            var tenantSetting = tenantScopedApplicationBuilder.ApplicationServices.GetService<ITenant>();
            var tenantName    = tenantSetting?.Name;
            try
            {
                // build the pipeline for tenant, it will be used to delegate all the requests later in next middleware
                var pipeline = tenantScopedApplicationBuilder.Build();

                _logger.LogInformation($"[{tenantName}] - Finished initializing.");

                // Now execute all the startup task before tenant actually runs
                // i.e.) data migration and etc.
                // Tenant does not start here yet
                _logger.LogInformation($"[{tenantName}] - Executing start up tasks");
                var startupTaskManager = tenantScopedApplicationBuilder
                                        .ApplicationServices
                                        .GetService<ITenantStartupTaskExecutor>();

                startupTaskManager.ExecuteAll().Wait();
                _logger.LogInformation($"[{tenantName}] - Finished executing start up tasks");

                return pipeline;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, $"[{tenantName}] - Error while initializing tenant");
                throw;
            }
        }

        private void ConfigureTenantPipeline(IApplicationBuilder tenantAppBuilder)
        {
            _configurationBuilder?.Configure?.Invoke(tenantAppBuilder);
        }
    }
}