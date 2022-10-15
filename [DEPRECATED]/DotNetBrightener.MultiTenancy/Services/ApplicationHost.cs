using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotNetBrightener.MultiTenancy.Contexts;
using DotNetBrightener.MultiTenancy.Extensions;
using Microsoft.Extensions.Logging;

namespace DotNetBrightener.MultiTenancy.Services
{
    /// <summary>
    /// All <see cref="Initialize"/> object are loaded when <see cref="TenantContext"/> is called. They can be removed when the
    /// tenant is removed, but are necessary to match an incoming request, even if they are not initialized.
    /// Each <see cref="TenantContext"/> is activated (its service provider is built) on the first request.
    /// </summary>
    public class ApplicationHost : IApplicationHost
    {
        private readonly ITenantManager _tenantSettingsManager;
        private readonly ITenantContextFactory  _tenantContextFactory;
        private readonly IRunningTenantTable    _runningTenantTable;
        private readonly ILogger                _logger;

        private readonly ConcurrentDictionary<string, TenantContext> _tenantContexts =
            new ConcurrentDictionary<string, TenantContext>();

        private DateTimeOffset _lastRefreshTime = DateTimeOffset.MinValue;

        private readonly HashSet<string> _restartingTenants = new HashSet<string>();

        public ApplicationHost(ITenantContextFactory    tenantContextFactory,
                               IRunningTenantTable      runningTenantTable,
                               ITenantManager   tenantSettingsManager,
                               ILogger<ApplicationHost> logger)
        {
            _tenantContextFactory  = tenantContextFactory;
            _runningTenantTable    = runningTenantTable;
            _tenantSettingsManager = tenantSettingsManager;
            _logger                = logger;
        }

        public void Initialize()
        {
            if (!_tenantContexts.Any() ||
                _lastRefreshTime == DateTimeOffset.MinValue ||
                _lastRefreshTime < _tenantSettingsManager.LastUpdatedTime)
            {
                lock (this)
                {
                    if (!_tenantContexts.Any() ||
                        _lastRefreshTime == DateTimeOffset.MinValue ||
                        _lastRefreshTime < _tenantSettingsManager.LastUpdatedTime)
                    {
                        _tenantContexts.Clear();
                        CreateAndRegisterTenants().Wait();
                        _lastRefreshTime = DateTimeOffset.Now;
                    }
                }
            }
        }

        public TenantContext GetOrCreateTenantContext(ITenant settings)
        {
            var shell = _tenantContexts.GetOrAdd(settings.Name,
                                                 _ =>
                                                 {
                                                     var context = CreateTenantContext(settings);
                                                     RegisterTenant(context);

                                                     return context;
                                                 });

            if (shell.NeedRestart)
            {
                _tenantContexts.TryRemove(settings.Name, out _);
                return GetOrCreateTenantContext(settings);
            }

            return shell;
        }

        public void UpdateTenantSetting(ITenant settings)
        {
            _tenantSettingsManager.SaveTenant(settings);
            ReloadTenant(settings);
        }

        public void RemoveTenant(ITenant settings)
        {
            if (_tenantContexts.TryRemove(settings.Name, out var context))
            {
                _runningTenantTable.Remove(settings);
                context.Release();
            }
        }

        async Task CreateAndRegisterTenants()
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Start creation of shells");
            }

            // Is there any tenant right now?
            var allSettings = _tenantSettingsManager.LoadTenants()
                                                    .ToArray();

            // No settings, run the Setup.
            if (allSettings.Length == 0)
            {
                var setupContext = await CreateSetupContextAsync();
                RegisterTenant(setupContext);
            }
            else
            {
                // Load all tenants, and activate their shell.
                Parallel.ForEach(allSettings, settings =>
                                              {
                                                  try
                                                  {
                                                      GetOrCreateTenantContext(settings);
                                                  }
                                                  catch (Exception ex)
                                                  {
                                                      _logger.LogError(ex,
                                                                       $"A tenant could not be started '{settings.Name}'");
                                                      throw;
                                                  }
                                              });
            }

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Done creating shells");
            }
        }

        public void ReloadTenant(ITenant settings)
        {
            if (_restartingTenants.Contains(settings.Name))
                return;

            lock (_restartingTenants)
            {
                if (_restartingTenants.Contains(settings.Name))
                    return;

                _restartingTenants.Add(settings.Name);

                if (_tenantContexts.TryRemove(settings.Name, out var context))
                {
                    _runningTenantTable.Remove(settings);
                    context.Release();
                }

                GetOrCreateTenantContext(settings);

                _restartingTenants.Remove(settings.Name);
            }
        }

        public IEnumerable<TenantContext> ListTenantContexts()
        {
            return _tenantContexts.Values;
        }

        /// <summary>
        /// Creates a shell context based on shell settings
        /// </summary>
        private TenantContext CreateTenantContext(ITenant settings)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("Creating shell context for tenant '{TenantName}'", settings.Name);
            }

            return _tenantContextFactory.CreateTenantContext(settings);
        }

        /// <summary>
        /// Creates a transient shell for the default tenant's setup.
        /// </summary>
        private Task<TenantContext> CreateSetupContextAsync()
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("Creating shell context for root setup.");
            }

            return Task.FromResult(_tenantContextFactory.CreateTenantContext(TenantHelpers.DefaultUninitializedTenant));
        }


        /// <summary>
        ///     Registers the tenant settings in RunningTenantTable
        /// </summary>
        private void RegisterTenant(TenantContext context)
        {
            if (_tenantContexts.TryAdd(context.Tenant.Name, context))
            {
                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    _logger.LogDebug($"Registering shell context for tenant '{context.Tenant.Name}'");
                }

                _runningTenantTable.Add(context.Tenant);
            }
        }
    }
}