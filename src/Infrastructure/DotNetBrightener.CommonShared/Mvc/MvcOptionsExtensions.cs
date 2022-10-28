using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetBrightener.CommonShared.Mvc;

public static class MvcOptionsExtensions
{
    public static void RegisterFilterProviders(this MvcOptions    mvcOptions,
                                               IServiceCollection serviceCollection)
    {
        var actionFilterTypes = serviceCollection.Where(_ => _.ServiceType == typeof(IActionFilterProvider))
                                                 .Where(_ => _.ImplementationType is
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