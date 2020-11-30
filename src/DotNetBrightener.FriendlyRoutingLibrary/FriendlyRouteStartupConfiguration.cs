using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DotNetBrightener.FriendlyRoutingLibrary
{
    public static class FriendlyRouteStartupConfiguration
    {
        public static void AddFriendlyRouting(this IServiceCollection services)
        {
            services.TryAdd(ServiceDescriptor.Singleton<FriendlyRoutingHandler, FriendlyRoutingHandler>());
            services.TryAdd(ServiceDescriptor.Singleton<FriendlyRouteActionSelector, FriendlyRouteActionSelector>());

            services.AddSingleton<IFrontEndRoutingEntries, FrontEndRoutingEntries>();
        }
    }
}