using System.Reflection;

namespace DotNetBrightener.Mapper.Dashboard;

/// <summary>
///     Discovers and catalogs all mapping types in the application.
/// </summary>
public static class MappingDiscovery
{
    private static readonly Lock                             Lock = new();
    private static          IReadOnlyList<MappingSourceInfo>? _cachedMappings;

    /// <summary>
    ///     Discovers all mappings from the specified assemblies.
    /// </summary>
    /// <param name="assemblies">
    ///     The assemblies to scan. If null, scans all loaded assemblies.
    /// </param>
    /// <returns>
    ///     A collection of mappings grouped by source type.
    /// </returns>
    public static IReadOnlyList<MappingSourceInfo> DiscoverMappingTypes(IEnumerable<Assembly>? assemblies = null)
    {
        lock (Lock)
        {
            if (_cachedMappings != null)
                return _cachedMappings;

            var assembliesToScan = assemblies ?? GetRelevantAssemblies();
            var mappings         = DiscoverMappingTypesCore(assembliesToScan);
            _cachedMappings = mappings;

            return mappings;
        }
    }

    /// <summary>
    ///     Clears the cached discovery results, forcing a re-scan on next call.
    /// </summary>
    public static void ClearCache()
    {
        lock (Lock)
        {
            _cachedMappings = null;
        }
    }

    /// <summary>
    ///     Discovers targets from the entry assembly and its referenced assemblies.
    /// </summary>
    public static IReadOnlyList<MappingSourceInfo> DiscoverFromEntryAssembly()
    {
        var entryAssembly = Assembly.GetEntryAssembly();

        if (entryAssembly == null)
            return Array.Empty<MappingSourceInfo>();

        var assemblies = new HashSet<Assembly>
        {
            entryAssembly
        };

        // Add referenced assemblies
        foreach (var reference in entryAssembly.GetReferencedAssemblies())
        {
            try
            {
                var assembly = Assembly.Load(reference);
                assemblies.Add(assembly);
            }
            catch
            {
                // Skip assemblies that can't be loaded
            }
        }

        return DiscoverMappingTypesCore(assemblies);
    }

    private static IEnumerable<Assembly> GetRelevantAssemblies()
    {
        // Get all currently loaded assemblies, filtering out system assemblies
        return AppDomain.CurrentDomain.GetAssemblies()
                        .Where(a => !a.IsDynamic && !IsSystemAssembly(a));
    }

    private static bool IsSystemAssembly(Assembly assembly)
    {
        var name = assembly.GetName().Name;

        if (name == null) return true;

        // Skip system and framework assemblies
        return name.StartsWith("System", StringComparison.OrdinalIgnoreCase) ||
               name.StartsWith("Microsoft", StringComparison.OrdinalIgnoreCase) ||
               name.StartsWith("mscorlib", StringComparison.OrdinalIgnoreCase) ||
               name.StartsWith("netstandard", StringComparison.OrdinalIgnoreCase) ||
               name.StartsWith("WindowsBase", StringComparison.OrdinalIgnoreCase);
    }

    private static IReadOnlyList<MappingSourceInfo> DiscoverMappingTypesCore(IEnumerable<Assembly> assemblies)
    {
        var targetsBySource = new Dictionary<Type, List<MappingTypeInfo>>();

        foreach (var assembly in assemblies)
        {
            try
            {
                foreach (var type in GetLoadableTypes(assembly))
                {
                    var targetAttributes = GetMappingTargetAttributes(type);

                    foreach (var attr in targetAttributes)
                    {
                        var sourceType = attr.SourceType;

                        if (sourceType == null) continue;

                        var targetInfo = CreateTargetTypeInfo(type, attr);

                        if (!targetsBySource.TryGetValue(sourceType, out var list))
                        {
                            list = new List<MappingTypeInfo>();
                            targetsBySource[sourceType] = list;
                        }

                        list.Add(targetInfo);
                    }
                }
            }
            catch
            {
                // Skip assemblies that can't be scanned
            }
        }

        // Build the final result
        var result = new List<MappingSourceInfo>();

        foreach (var kvp in targetsBySource.OrderBy(x => x.Key.FullName))
        {
            var sourceMembers = GetMembersFromType(kvp.Key);
            var mapping       = new MappingSourceInfo(kvp.Key, kvp.Value, sourceMembers);
            result.Add(mapping);
        }

        return result.AsReadOnly();
    }

    private static IReadOnlyList<MappingTargetAttributeInfo> GetMappingTargetAttributes(MemberInfo member)
    {
        return member.GetCustomAttributesData()
            .Where(IsMappingTargetAttribute)
            .Select(CreateMappingTargetAttributeInfo)
            .Where(info => info is not null)
            .Cast<MappingTargetAttributeInfo>()
            .ToList()
            .AsReadOnly();
    }

    private static bool IsMappingTargetAttribute(CustomAttributeData attributeData)
    {
        var attributeType = attributeData.AttributeType;
        return attributeType.IsGenericType &&
               attributeType.GetGenericTypeDefinition() == typeof(MappingTargetAttribute<>);
    }

    private static MappingTargetAttributeInfo? CreateMappingTargetAttributeInfo(CustomAttributeData attributeData)
    {
        var sourceType = attributeData.AttributeType.GenericTypeArguments.FirstOrDefault();
        if (sourceType is null)
        {
            return null;
        }

        return new MappingTargetAttributeInfo(
            SourceType: sourceType,
            Exclude: GetConstructorStringArray(attributeData, 0),
            Include: GetNamedStringArray(attributeData, "Include"),
            NullableProperties: GetNamedValue(attributeData, "NullableProperties", false),
            CopyAttributes: GetNamedValue(attributeData, "CopyAttributes", false),
            Configuration: GetNamedTypeValue(attributeData, "Configuration"),
            NestedTargetTypes: GetNamedTypeArray(attributeData, "NestedTargetTypes"));
    }

    private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            return ex.Types.Where(t => t != null)!;
        }
    }

    private static MappingTypeInfo CreateTargetTypeInfo(Type targetType, MappingTargetAttributeInfo attr)
    {
        // Determine type kind
        var typeKind = DetermineTypeKind(targetType);

        // Check for constructor from source type
        var hasConstructor = targetType.GetConstructor(new[]
                             {
                                 attr.SourceType
                             }) !=
                             null;

        // Check for Projection property
        var hasProjection = targetType.GetProperty("Projection", BindingFlags.Public | BindingFlags.Static) != null;

        // Check for ToSource method
        var hasToSource =
            targetType.GetMethod("ToSource", BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null) !=
            null;

        // Get members
        var members = GetMembersFromType(targetType);

        // Get nested targets
        var nestedTargets = attr.NestedTargetTypes ?? Array.Empty<Type>();

        // Configuration type
        var configTypeName = attr.Configuration?.FullName;

        return new MappingTypeInfo(
                                 targetType: targetType,
                                 hasConstructor: hasConstructor,
                                 hasProjection: hasProjection,
                                 hasToSource: hasToSource,
                                 excludedProperties: attr.Exclude ?? Array.Empty<string>(),
                                 includedProperties: attr.Include,
                                 members: members,
                                 nestedTargets: nestedTargets,
                                 typeKind: typeKind,
                                 nullableProperties: attr.NullableProperties,
                                 copyAttributes: attr.CopyAttributes,
                                 configurationTypeName: configTypeName
                                );
    }

    private static string DetermineTypeKind(Type type)
    {
        if (type.IsValueType)
        {
            // Check for record struct by looking for <Clone>$ method
            var cloneMethod = type.GetMethod("<Clone>$", BindingFlags.Public | BindingFlags.Instance);

            if (cloneMethod != null)
                return "record struct";

            return "struct";
        }
        else
        {
            // Check for record by looking for <Clone>$ method
            var cloneMethod = type.GetMethod("<Clone>$", BindingFlags.Public | BindingFlags.Instance);

            if (cloneMethod != null)
                return "record";

            return "class";
        }
    }

    private static IReadOnlyList<MappingMemberInfo> GetMembersFromType(Type type)
    {
        var members = new List<MappingMemberInfo>();

        // Get properties
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var prop in properties)
        {
            var memberInfo = CreateMemberInfo(prop);
            members.Add(memberInfo);
        }

        // Get public fields
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);

        foreach (var field in fields)
        {
            var memberInfo = CreateMemberInfo(field);
            members.Add(memberInfo);
        }

        return members.AsReadOnly();
    }

    private static MappingMemberInfo CreateMemberInfo(PropertyInfo prop)
    {
        var propertyType = prop.PropertyType;
        var isNullable = IsNullableType(propertyType) ||
                         HasNullableAttribute(prop);

        var isReadOnly = !prop.CanWrite;
        var isInitOnly = IsInitOnlyProperty(prop);
        var isRequired = HasRequiredAttribute(prop);

        // Check for MapFrom attribute
        var mapFromAttr = prop.GetCustomAttributes()
                              .FirstOrDefault(a => a.GetType().Name == "MapFromAttribute");

        string? mappedFromProperty = null;

        if (mapFromAttr != null)
        {
            var sourceProperty = mapFromAttr.GetType().GetProperty("Source");
            mappedFromProperty = sourceProperty?.GetValue(mapFromAttr) as string;
        }

        var attributes = prop.GetCustomAttributes(inherit: true)
                             .Select(a => a.GetType().Name.Replace("Attribute", ""))
                             .ToList();

        var isNestedTarget = HasMappingTargetAttribute(propertyType);
        var isCollection  = IsCollectionType(propertyType);

        return new MappingMemberInfo(
                                   name: prop.Name,
                                   typeName: GetFriendlyTypeName(propertyType),
                                   isProperty: true,
                                   isNullable: isNullable,
                                   isRequired: isRequired,
                                   isInitOnly: isInitOnly,
                                   isReadOnly: isReadOnly,
                                   xmlDocumentation: null, // Would need XML documentation file
                                   attributes: attributes,
                                   isNestedTarget: isNestedTarget,
                                   isCollection: isCollection,
                                   mappedFromProperty: mappedFromProperty
                                  );
    }

    private static MappingMemberInfo CreateMemberInfo(FieldInfo field)
    {
        var fieldType = field.FieldType;
        var isNullable = IsNullableType(fieldType) ||
                         HasNullableAttribute(field);

        var isRequired = HasRequiredAttribute(field);

        var attributes = field.GetCustomAttributes(inherit: true)
                              .Select(a => a.GetType().Name.Replace("Attribute", ""))
                              .ToList();

        var isNestedTarget = HasMappingTargetAttribute(fieldType);
        var isCollection  = IsCollectionType(fieldType);

        return new MappingMemberInfo(
                                   name: field.Name,
                                   typeName: GetFriendlyTypeName(fieldType),
                                   isProperty: false,
                                   isNullable: isNullable,
                                   isRequired: isRequired,
                                   isInitOnly: field.IsInitOnly,
                                   isReadOnly: field.IsInitOnly,
                                   xmlDocumentation: null,
                                   attributes: attributes,
                                   isNestedTarget: isNestedTarget,
                                   isCollection: isCollection,
                                   mappedFromProperty: null
                                  );
    }

    private static bool IsNullableType(Type type)
    {
        return Nullable.GetUnderlyingType(type) != null ||
               (!type.IsValueType && type.IsClass);
    }

    private static bool HasNullableAttribute(MemberInfo member)
    {
        // Check for nullable reference type annotations
        var nullableAttr = member.CustomAttributes
                                 .FirstOrDefault(a => a.AttributeType.Name == "NullableAttribute");

        if (nullableAttr?.ConstructorArguments.Count > 0)
        {
            var arg = nullableAttr.ConstructorArguments[0];

            if (arg.Value is byte b)
                return b == 2; // 2 = nullable
            if (arg.Value is byte[] bytes &&
                bytes.Length > 0)
                return bytes[0] == 2;
        }

        return false;
    }

    private static bool IsInitOnlyProperty(PropertyInfo prop)
    {
        var setMethod = prop.SetMethod;

        if (setMethod == null) return false;

        // Check for init accessor by looking for modreq
        var returnParam = setMethod.ReturnParameter;
        var modreqs     = returnParam?.GetRequiredCustomModifiers();

        return modreqs?.Any(m => m.Name == "IsExternalInit") == true;
    }

    private static bool HasRequiredAttribute(MemberInfo member)
    {
        return member.CustomAttributes
                     .Any(a => a.AttributeType.Name == "RequiredMemberAttribute");
    }

    private static bool IsCollectionType(Type type)
    {
        if (type == typeof(string)) return false;
        if (type.IsArray) return true;

        return type.GetInterfaces()
                   .Any(i => i.IsGenericType &&
                             (i.GetGenericTypeDefinition() == typeof(IEnumerable<>) ||
                              i.GetGenericTypeDefinition() == typeof(ICollection<>) ||
                              i.GetGenericTypeDefinition() == typeof(IList<>)));
    }

    private static string GetFriendlyTypeName(Type type)
    {
        if (type.IsGenericType)
        {
            var genericType = type.GetGenericTypeDefinition();
            var args        = type.GetGenericArguments();
            var argsStr     = string.Join(", ", args.Select(GetFriendlyTypeName));

            var typeName      = genericType.Name;
            var backtickIndex = typeName.IndexOf('`');
            if (backtickIndex > 0)
                typeName = typeName.Substring(0, backtickIndex);

            // Handle nullable value types
            if (genericType == typeof(Nullable<>))
                return $"{GetFriendlyTypeName(args[0])}?";

            return $"{typeName}<{argsStr}>";
        }

        if (type.IsArray)
        {
            var elementType = type.GetElementType()!;

            return $"{GetFriendlyTypeName(elementType)}[]";
        }

        // Common type aliases
        return type.FullName switch
        {
            "System.String"  => "string",
            "System.Int32"   => "int",
            "System.Int64"   => "long",
            "System.Int16"   => "short",
            "System.Byte"    => "byte",
            "System.Boolean" => "bool",
            "System.Decimal" => "decimal",
            "System.Double"  => "double",
            "System.Single"  => "float",
            "System.Object"  => "object",
            "System.Void"    => "void",
            _                => type.Name
        };
    }

    private static bool HasMappingTargetAttribute(Type type)
    {
        return type.GetCustomAttributesData().Any(IsMappingTargetAttribute);
    }

    private static string[] GetConstructorStringArray(CustomAttributeData attributeData, int index)
    {
        if (attributeData.ConstructorArguments.Count <= index)
        {
            return Array.Empty<string>();
        }

        var argument = attributeData.ConstructorArguments[index];
        if (argument.ArgumentType == typeof(string))
        {
            return argument.Value is string value ? new[] { value } : Array.Empty<string>();
        }

        if (argument.ArgumentType.IsArray && argument.Value is IReadOnlyCollection<CustomAttributeTypedArgument> values)
        {
            return values
                .Select(value => value.Value as string)
                .Where(value => value is not null)
                .Cast<string>()
                .ToArray();
        }

        return Array.Empty<string>();
    }

    private static string[]? GetNamedStringArray(CustomAttributeData attributeData, string name)
    {
        var argument = attributeData.NamedArguments.FirstOrDefault(arg => arg.MemberName == name);
        if (argument.MemberInfo is null)
        {
            return null;
        }

        if (argument.TypedValue.ArgumentType == typeof(string))
        {
            return argument.TypedValue.Value is string value ? new[] { value } : Array.Empty<string>();
        }

        if (argument.TypedValue.ArgumentType.IsArray &&
            argument.TypedValue.Value is IReadOnlyCollection<CustomAttributeTypedArgument> values)
        {
            return values
                .Select(value => value.Value as string)
                .Where(value => value is not null)
                .Cast<string>()
                .ToArray();
        }

        return null;
    }

    private static Type[] GetNamedTypeArray(CustomAttributeData attributeData, string name)
    {
        var argument = attributeData.NamedArguments.FirstOrDefault(arg => arg.MemberName == name);
        if (argument.MemberInfo is null ||
            !argument.TypedValue.ArgumentType.IsArray ||
            argument.TypedValue.Value is not IReadOnlyCollection<CustomAttributeTypedArgument> values)
        {
            return Array.Empty<Type>();
        }

        return values
            .Select(value => value.Value as Type)
            .Where(value => value is not null)
            .Cast<Type>()
            .ToArray();
    }

    private static Type? GetNamedTypeValue(CustomAttributeData attributeData, string name)
    {
        var argument = attributeData.NamedArguments.FirstOrDefault(arg => arg.MemberName == name);
        return argument.TypedValue.Value as Type;
    }

    private static T GetNamedValue<T>(CustomAttributeData attributeData, string name, T defaultValue)
    {
        var argument = attributeData.NamedArguments.FirstOrDefault(arg => arg.MemberName == name);
        return argument.TypedValue.Value is T value ? value : defaultValue;
    }

    private sealed record MappingTargetAttributeInfo(
        Type SourceType,
        string[] Exclude,
        string[]? Include,
        bool NullableProperties,
        bool CopyAttributes,
        Type? Configuration,
        Type[] NestedTargetTypes);
}
