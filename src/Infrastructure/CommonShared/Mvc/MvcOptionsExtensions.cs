using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace WebApp.CommonShared.Mvc;

public static class MvcOptionsExtensions
{
    /// <summary>
    ///     Detects and registers all the <see cref="IActionFilterProvider"/> implementations as global action filters
    ///     to the specified <see cref="MvcOptions"/>
    /// </summary>
    /// <param name="mvcOptions">
    ///     The <see cref="MvcOptions"/> to register the action filters to
    /// </param>
    /// <param name="serviceCollection">
    ///     The <see cref="IServiceCollection"/> to detect the <see cref="IActionFilterProvider"/> implementations from
    /// </param>
    public static void RegisterFilterProviders(this MvcOptions    mvcOptions,
                                               IServiceCollection serviceCollection)
    {
        var actionFilterTypes = serviceCollection.Where(_ => _.ServiceType == typeof(IActionFilterProvider) &&
                                                             _.ImplementationType is
                                                                 { IsInterface: false, IsAbstract: false })
                                                 .Select(_ => _.ImplementationType)
                                                 .ToList();

        if (!actionFilterTypes.Any())
            return;

        foreach (var actionFilterProviderType in actionFilterTypes)
        {
            mvcOptions.Filters.Add(actionFilterProviderType);
        }
    }
}