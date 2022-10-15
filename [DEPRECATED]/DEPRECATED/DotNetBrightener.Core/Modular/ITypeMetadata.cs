using System;

namespace DotNetBrightener.Core.Modular;

/// <summary>
///     Represents the metadata for particular <see cref="Type" />
/// </summary>
public interface ITypeMetadata
{
    Type Type { get; }

    ModuleEntry AssociatedModule { get; set; }

    string SubArea { get; }
}