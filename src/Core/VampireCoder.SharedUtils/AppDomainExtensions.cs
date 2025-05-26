// ReSharper disable CheckNamespace

using VampireCoder.SharedUtils;

namespace System.Reflection;

public static class AppDomainExtensions
{
    /// <summary>
    ///     Retrieves the assemblies that are loaded for the running application but skips the system assemblies
    /// </summary>
    /// <param name="appDomain">
    ///     The application domain
    /// </param>
    /// <returns>
    ///     An array of non-system assemblies that are loaded for the specified application domain
    /// </returns>
    public static Assembly[] GetAppOnlyAssemblies(this AppDomain appDomain)
    {
        return appDomain.GetAssemblies()
                        .FilterSkippedAssemblies()
                        .ToArray();
    }

}