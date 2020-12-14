using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DotNetBrightener.FriendlyRoutingLibrary
{
    public static class FriendlyRouteStartupConfiguration
    {
        /// <summary>
        ///     Enables the Friendly Routing Handler into the <paramref name="services"/>
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection" /></param>
        public static void AddFriendlyRouting(this IServiceCollection services)
        {
            services.TryAdd(ServiceDescriptor.Singleton<FriendlyRoutingHandler, FriendlyRoutingHandler>());
            services.TryAdd(ServiceDescriptor.Singleton<FriendlyRouteActionSelector, FriendlyRouteActionSelector>());

            services.AddSingleton<IFrontEndRoutingEntries, FrontEndRoutingEntries>();
        }
    }
}