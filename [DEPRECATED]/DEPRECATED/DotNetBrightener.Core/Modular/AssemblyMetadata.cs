using System.Reflection;

namespace DotNetBrightener.Core.Modular;

public class AssemblyMetadata
{
    public Assembly Assembly { get; set; }

    public ModuleEntry AssociatedModule { get; set; }
}