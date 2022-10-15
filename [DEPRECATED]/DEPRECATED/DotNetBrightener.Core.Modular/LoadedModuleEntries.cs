using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DotNetBrightener.Core.Modular
{
    /// <summary>
    ///     The collection of modules that are loaded for a context eg: tenant or application
    /// </summary>
    public class LoadedModuleEntries : List<ModuleEntry>
    {
        public LoadedModuleEntries()
        {

        }

        public LoadedModuleEntries(IEnumerable<ModuleEntry> moduleEntries) : base(moduleEntries)
        {

        }

        /// <summary>
        /// Retrieves the assemblies loaded for the context
        /// </summary>
        /// <returns>An array of <see cref="Assembly"/> loaded for the context</returns>
        public Assembly[] GetModuleAssemblies()
        {
            return this.SelectMany(_ => _.ModuleAssemblies)
                       .ToArray();
        }

        /// <summary>
        ///     Retrieves the exported types loaded for the context, excluding abstract classes and interfaces
        /// </summary>
        /// <returns>An array of <see cref="Type"/> loaded for the context</returns>
        public Type[] GetExportedTypes()
        {
            return this.SelectMany(_ => _.ModuleTypeMetadataCollection)
                       .Where(_ => _.Type.IsNotSystemType() && !_.IsAbstract && !_.IsInterface)
                       .Select(_ => _.Type)
                       .Distinct()
                       .ToArray();
        }

        /// <summary>
        /// Retrieves the exported types derived from <typeparamref name="T"/> which loaded for the context, excluding abstract classes and interfaces
        /// </summary>
        /// <returns>An array of <see cref="Type"/> in the given type loaded for the context</returns>
        public Type[] GetExportedTypesOfType<T>()
        {
            return this.SelectMany(_ => _.ModuleTypeMetadataCollection)
                       .Where(_ => !_.IsAbstract && !_.IsInterface &&
                                   (_.InterfacesList.Contains(typeof(T)) || _.Type.BaseType == typeof(T)))
                       .Select(_ => _.Type)
                       .Distinct()
                       .ToArray();
        }

        /// <summary>
        /// Retrieves the exported types derived from <typeparamref name="T"/> which loaded for the context, excluding abstract classes and interfaces
        /// </summary>
        /// <returns>An array of <see cref="Type"/> in the given type loaded for the context</returns>
        public Type[] GetExportedTypesWithName(string typeName)
        {
            return this.SelectMany(_ => _.ModuleTypeMetadataCollection)
                       .Where(_ => !_.IsAbstract && !_.IsInterface && _.TypeName == typeName)
                       .Select(_ => _.Type)
                       .Distinct()
                       .ToArray();
        }
    }
}