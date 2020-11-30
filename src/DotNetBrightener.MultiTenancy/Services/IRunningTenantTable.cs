using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using DotNetBrightener.MultiTenancy.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

namespace DotNetBrightener.MultiTenancy.Services
{
    public interface IRunningTenantTable
    {
        void Add(ITenant tenant);

        void Remove(ITenant tenant);

        ITenant DetectTenantFromContext(HttpContext httpContext);
    }

    public class RunningTenantTable : IRunningTenantTable
    {
        private readonly ConcurrentDictionary<string, ITenant> _tenantsByHost =  new ConcurrentDictionary<string, ITenant>(StringComparer.OrdinalIgnoreCase);

        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
        private readonly ITenantManager _tenantManager;

        private IEnumerable<ITenant> AllTenants => _tenantManager.LoadTenants();

        public RunningTenantTable(ITenantManager tenantManager)
        {
            _tenantManager = tenantManager;
        }

        public void Add(ITenant tenant)
        {
            _lock.EnterWriteLock();

            try
            {
                var allHostValues = tenant.ParseHostValues();
                foreach (var hostName in allHostValues)
                {
                    _tenantsByHost.TryAdd(hostName, tenant);
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void Remove(ITenant tenant)
        {
            _lock.EnterWriteLock();

            try
            {
                var allHostValues = tenant.ParseHostValues();
                foreach (var hostName in allHostValues)
                {
                    _tenantsByHost.TryRemove(hostName, out _);
                }

            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public ITenant DetectTenantFromContext(HttpContext httpContext)
        {
            string host = httpContext?.Request?.Headers[HeaderNames.Host];

            if (_tenantsByHost.TryGetValue(host, out var tenant))
                return tenant;

            return null;
        }
    }
}