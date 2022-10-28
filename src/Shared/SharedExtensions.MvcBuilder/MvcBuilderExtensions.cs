using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection;

internal static class MvcBuilderExtensions
{
    public static void RegisterMeAsMvcModule(this IMvcBuilder mvcBuilder)
    {
        var callingAssembly = Assembly.GetCallingAssembly();

        mvcBuilder.AddApplicationPart(callingAssembly);
    }
}