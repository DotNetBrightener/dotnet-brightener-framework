using System;
using System.Collections.Concurrent;
using Microsoft.AspNetCore.Http;

namespace DotNetBrightener.MultiTenancy.MiddleWares
{
    public interface ITenantPipelineContainer
    {
        /// <summary>
        /// Retrieves the <see cref="RequestDelegate"/> to service
        /// </summary>
        /// <param name="tenantName"></param>
        /// <returns></returns>
        RequestDelegate GetPipeline(string tenantName);

        void AddPipeline(string tenantName, RequestDelegate tenantPipeline);

        bool ContainsPipelineForTenant(string tenantName);

        void RemovePipeline(string tenantName);
    }

    public class TenantPipelineContainer : ITenantPipelineContainer
    {
        private readonly ConcurrentDictionary<string, RequestDelegate> _pipelines =
            new ConcurrentDictionary<string, RequestDelegate>();

        public RequestDelegate GetPipeline(string tenantName)
        {
            lock (_pipelines)
            {
                if (_pipelines.TryGetValue(tenantName, out var pipeline))
                    return pipeline;

                return null;
            }
        }

        public void AddPipeline(string tenantName, RequestDelegate tenantPipeline)
        {
            lock (_pipelines)
            {
                // try add, if fail throw exception
                if (!_pipelines.TryAdd(tenantName, tenantPipeline))
                    throw new
                        InvalidOperationException($"Cannot add pipeline for tenant {tenantName}. It may already exist.");
            }
        }

        public bool ContainsPipelineForTenant(string tenantName)
        {
            lock (_pipelines)
            {
                return _pipelines.ContainsKey(tenantName);
            }
        }

        public void RemovePipeline(string tenantName)
        {
            lock (_pipelines)
            {
                _pipelines.TryRemove(tenantName, out _);
            }
        }
    }
}