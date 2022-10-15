using System;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetBrightener.MultiTenancy
{
    internal static class MultiTenantConfigurator
    {
        public static Type TenantType { get; set; }

        public static Action<object, IServiceCollection> ConfigureTenant { get; internal set; }
    }
}