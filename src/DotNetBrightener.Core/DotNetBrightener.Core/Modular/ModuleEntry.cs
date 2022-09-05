using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Newtonsoft.Json;

namespace DotNetBrightener.Core.Modular;

public class ModuleEntry: ModuleDefinition
{
    public const string MainModuleIdentifier = "MainModule";

    public ModuleEntry(): base()
    {
        Dependencies     = new List<string>();
        ModuleAssemblies = new List<Assembly>();
    }

    public ModuleEntry(ModuleDefinition moduleDefinition) : this()
    {
        moduleDefinition.CopyTo(this);
    }

    /// <summary>
    ///     The path to module
    /// </summary>
    public string ModuleFolderPath { get; set; }

    /// <summary>
    /// The assemblies that are populated by the module
    /// </summary>
    [JsonIgnore]
    public List<Assembly> ModuleAssemblies { get; set; } = new List<Assembly>();

    private IFileProvider _moduleFileProvider;

    /// <summary>
    /// The FileProvider for accessing the modules' files
    /// </summary>
    [JsonIgnore]
    public IFileProvider ModuleFileProvider
    {
        get
        {
            if (_moduleFileProvider != null)
                return _moduleFileProvider;

            _moduleFileProvider = new PhysicalFileProvider(ModuleFolderPath);
            return _moduleFileProvider;
        }
    }

    private IFileProvider _staticFileProvider;

    /// <summary>
    /// The FileProvider for accessing the static files of the modules
    /// </summary>
    [JsonIgnore]
    public IFileProvider StaticFileProvider
    {
        get
        {
            if (_staticFileProvider != null)
                return _staticFileProvider;

            var wwwrootFolderName = "wwwroot";
            var wwwrootDir        = new DirectoryInfo(Path.Combine(ModuleFolderPath, wwwrootFolderName));
            if (!wwwrootDir.Exists)
            {
                wwwrootDir = new DirectoryInfo(Path.Combine(BinPath, wwwrootFolderName));
            }

            if (!wwwrootDir.Exists)
                return null;

            _staticFileProvider = new PhysicalFileProvider(wwwrootDir.FullName);

            return _staticFileProvider;
        }
    }

    public void UseStaticFileProvider(IFileProvider fileProvider)
    {
        _staticFileProvider = fileProvider;
    }

    /// <summary>
    /// The associated metadata to each type of the module assemblies
    /// </summary>
    [JsonIgnore]
    public List<TypeMetadata> ModuleTypeMetadataCollection
    {
        get
        {
            if (_moduleTypeMetadataCollection != null)
                return _moduleTypeMetadataCollection;

            var types = new List<Type>();

            foreach (var moduleAssembly in ModuleAssemblies)
            {
                try
                {
                    types.AddRange(moduleAssembly.GetExportedTypes());
                }
                catch (ReflectionTypeLoadException exception)
                {
                    if (exception.Types != null && exception.Types.Any())
                        types.AddRange(exception.Types);
                }
                catch (TypeLoadException exception)
                {
                }
                catch (Exception exception)
                {

                }
            }

            types = types.Distinct(new TypeComparisonEquality()).ToList();

            _moduleTypeMetadataCollection = types.Select(_ => TypeMetadata.AssociateTypeWithModule(_, this))
                                                 .Where(_ => _ != null)
                                                 .ToList();

            return _moduleTypeMetadataCollection;
        }
    }

    private List<TypeMetadata> _moduleTypeMetadataCollection;

    public string BinPath { get; set; }

    [JsonIgnore]
    public IConfiguration Configuration { get; set; }

    /// <summary>
    ///     Retrieves the dependencies of the module
    /// </summary>
    [JsonIgnore]
    public List<ModuleEntry> DependencyModules { get; internal set; }
}