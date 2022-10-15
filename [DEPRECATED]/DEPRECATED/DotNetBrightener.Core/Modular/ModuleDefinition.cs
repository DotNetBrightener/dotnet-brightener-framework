
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace DotNetBrightener.Core.Modular;

/// <summary>
/// Represents the description of a module
/// </summary>
public class ModuleDefinition
{
    /// <summary>
    ///     Identifier of the module, should be unique. Can be something in format 'com.[company-name].[module-name]
    ///     <para>This will be used in the dependencies of other modules if specified</para>
    /// </summary>
    public string ModuleId { get; set; }
        
    /// <summary>
    ///     A friendly route for the module if needed
    /// </summary>
    public string Alias { get; set; }

    /// <summary>
    ///     Indicates if the module supports or provides Single-Page-Application
    /// </summary>
    public bool EnableSpa { get; set; }

    /// <summary>
    ///     Specifies the version of the module, for upgrading or downgrading purposes
    /// </summary>
    public string Version { get; set; }

    [JsonIgnore]
    public Version VersionInfo
    {
        get => System.Version.Parse(Version ?? "1.0.0.0");
        set => Version = value.ToString();
    }

    /// <summary>
    ///     Friendly name of the module
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///     Description for the module of what it does etc.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    ///     Specifies the author of this module.
    /// </summary>
    public string Author { get; set; }

    /// <summary>
    ///     Copyright info
    /// </summary>
    public string Copyright { get; set; }

    /// <summary>
    ///     Type of module
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public ModuleType ModuleType { get; set; }

    /// <summary>
    ///     List of other modules' identifiers which this module depends on
    /// </summary>
    public List<string> Dependencies { get; set; }

    public ModuleDefinition()
    {
        ModuleType = ModuleType.ExtensionModule;
    }
}