using System.Reflection;

namespace WebApp.CommonShared.Endpoints;

/// <summary>
///     Configuration options for endpoint module discovery and registration.
/// </summary>
public class EndpointModuleOptions
{
    /// <summary>
    ///     Gets or sets the assemblies to scan for endpoint modules.
    /// </summary>
    public Assembly[] Assemblies { get; set; } = Array.Empty<Assembly>();

    /// <summary>
    ///     Gets or sets whether to auto-register validators from scanned assemblies.
    ///     Default is true.
    /// </summary>
    public bool AutoRegisterValidators { get; set; } = true;
}
