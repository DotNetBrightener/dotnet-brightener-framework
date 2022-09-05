using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace DotNetBrightener.PublisherTool.CLI;

/// <summary>
///     Represents the description of a module
/// </summary>
public class ModuleDefinition
{
    /// <summary>
    ///     Identifier of the module, should be unique.
    /// <para>
    ///     Can be something in format 'com.[company-name].[module-name]'
    /// </para>
    /// <para>
    ///     This will be used in the dependencies of other modules if specified
    /// </para>
    /// </summary>
    public string ModuleId { get; set; }

    /// <summary>
    ///     Friendly name of the module
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///     List of other modules' identifiers which this module depends on
    /// </summary>
    public List<string> Dependencies { get; set; } = new List<string>();

    /// <summary>
    /// Type of module
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public ModuleType ModuleType { get; set; }

    internal string AssociatedProjectFile { get; set; }

    internal FileInfo[] OutputDllFiles { get; set; }
}