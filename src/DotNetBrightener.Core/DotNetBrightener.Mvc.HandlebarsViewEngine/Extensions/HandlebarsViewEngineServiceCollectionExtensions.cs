using DotNetBrightener.Mvc.HandlebarsViewEngine.ViewEngines;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DotNetBrightener.Mvc.HandlebarsViewEngine.Extensions
{
    public static class HandlebarsViewEngineServiceCollectionExtensions
    {
        public static void AddHandleBarsViewEngine(this IServiceCollection serviceCollection)
        {
            // register customized view engine
            serviceCollection.AddSingleton<IHandlebarsViewEngine, DefaultHandlebarsViewEngine>();
            serviceCollection.AddTransient<IConfigureOptions<HandlebarsMvcViewOptions>, HandleBarViewOptionsSetup>();
            serviceCollection.AddTransient<IConfigureOptions<MvcViewOptions>, HandleBarMvcViewOptionsSetup>();
        }
    }
}
