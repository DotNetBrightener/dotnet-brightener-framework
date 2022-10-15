using System;
using Microsoft.AspNetCore.Mvc;

namespace DotNetBrightener.Core.Modular
{
    /// <summary>
    ///     Represents the metadata for particular <see cref="Type" />
    /// </summary>
    public interface ITypeMetadata
    {
        Type Type { get; }

        ModuleEntry AssociatedModule { get; set; }

        string SubArea { get; }
    }

    public class TypeMetadata : AssemblyMetadata, ITypeMetadata
    {
        public Type Type { get; private set; }

        /// <summary>
        /// Retrieves the name of type
        /// </summary>
        public string TypeName => Type?.Name;

        /// <summary>
        /// Retrieves the namespace of type
        /// </summary>
        public string TypeNamespace => Type?.Namespace;

        /// <summary>
        ///     Indicates the type is abstract
        /// </summary>
        public bool IsAbstract => Type.IsAbstract;

        /// <summary>
        ///     Indicates the type is interface
        /// </summary>
        public bool IsInterface => Type.IsAbstract;

        public Type[] InterfacesList => Type.GetInterfaces();

        /// <summary>
        ///     Indicates the type is a generic type
        /// </summary>
        public bool IsGenericType => Type.IsGenericType;

        private string _subAreaName;

        /// <summary>
        ///     Retrieves the Area of the controller, which considers as a subarea inside the module
        /// </summary>
        public string SubArea
        {
            get
            {
                if (!string.IsNullOrEmpty(_subAreaName))
                    return _subAreaName;

                var controllerType = Type;

                if (Type.BaseType != typeof(ControllerBase) &&
                    Type.BaseType != typeof(Controller))
                    return null;

                if (controllerType.Namespace != null && controllerType.Namespace.Contains(".Areas."))
                {
                    var subAreaSplits =
                        controllerType.Namespace.Split(new[] { ".Areas." }, StringSplitOptions.RemoveEmptyEntries);

                    if (subAreaSplits.Length == 2)
                    {
                        _subAreaName = subAreaSplits[1].Replace(".Controllers", "");
                        return _subAreaName;
                    }
                }

                return null;
            }
        }

        /// <summary>
        /// Associates the given type to specified module
        /// </summary>
        /// <param name="type">Type to associate to a module</param>
        /// <param name="moduleEntry">The module to associate the type to</param>
        /// <returns>A <see cref="TypeMetadata"/> object</returns>
        internal static TypeMetadata AssociateTypeWithModule(Type type, ModuleEntry moduleEntry)
        {
            var metadataInstance = new TypeMetadata
            {
                Type             = type,
                Assembly         = type.Assembly,
                AssociatedModule = moduleEntry
            };

            return metadataInstance;
        }
    }
}