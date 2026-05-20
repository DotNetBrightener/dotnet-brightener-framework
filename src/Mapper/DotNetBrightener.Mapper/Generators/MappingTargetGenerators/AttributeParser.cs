using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using DotNetBrightener.Mapper.Generators.Shared;
using Microsoft.CodeAnalysis;

namespace DotNetBrightener.Mapper.Generators.MappingTargetGenerators;

/// <summary>
///     Handles parsing of MappingTarget attribute data and extraction of configuration values.
/// </summary>
internal static class AttributeParser
{
    private const string MappingTargetAttributeName = "MappingTargetAttribute";
    private const string MappingTargetNamespace = "DotNetBrightener.Mapper";
    private const string WrapperAttributeName = "WrapperAttribute";

    private static bool IsMappingTargetAttributeClass(INamedTypeSymbol? attributeClass)
    {
        return attributeClass is not null &&
               attributeClass.Arity == 1 &&
               attributeClass.Name == MappingTargetAttributeName &&
               attributeClass.ContainingNamespace?.ToDisplayString() == MappingTargetNamespace;
    }

    public static bool IsMappingTargetAttribute(AttributeData attribute)
    {
        return IsMappingTargetAttributeClass(attribute.AttributeClass);
    }

    public static INamedTypeSymbol? ExtractSourceType(AttributeData attribute)
    {
        var attributeClass = attribute.AttributeClass;
        if (!IsMappingTargetAttributeClass(attributeClass))
        {
            return null;
        }

        return attributeClass.TypeArguments.Length > 0
            ? attributeClass.TypeArguments[0] as INamedTypeSymbol
            : null;
    }

    private static int GetExcludeArgumentIndex(AttributeData attribute)
    {
        if (IsMappingTargetAttribute(attribute))
        {
            return 0;
        }

        var attributeClass = attribute.AttributeClass;
        if (attributeClass?.Name == WrapperAttributeName &&
            attributeClass.ContainingNamespace?.ToDisplayString() == MappingTargetNamespace)
        {
            return 1;
        }

        return 0;
    }

    /// <summary>
    ///     Extracts nested target mappings from the NestedTargetTypes parameter.
    ///     Returns a dictionary mapping source type full names to nested target type information.
    /// </summary>
    public static Dictionary<string, (string childTargetTypeName, string sourceTypeName)> ExtractNestedTargetMappings(
        AttributeData attribute,
        Compilation compilation)
    {
        var mappings = new Dictionary<string, (string, string)>();

        var childrenArg = attribute.NamedArguments.FirstOrDefault(kvp => kvp.Key == Constants.AttributeNames.NestedTargetTypes);
        if (childrenArg.Value.Kind != TypedConstantKind.Error && !childrenArg.Value.IsNull)
        {
            if (childrenArg.Value.Kind == TypedConstantKind.Array)
            {
                foreach (var childValue in childrenArg.Value.Values)
                {
                    if (childValue.Value is INamedTypeSymbol childTargetType)
                    {
        // Find the MappingTarget attribute on the child type to get its source type
                        var childTargetAttr = childTargetType.GetAttributes()
                            .FirstOrDefault(IsMappingTargetAttribute);

                        var childSourceType = childTargetAttr is null
                            ? null
                            : ExtractSourceType(childTargetAttr);

                        if (childSourceType != null)
                        {
                            var sourceTypeName = childSourceType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                            var childTargetTypeName = childTargetType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

                            // Map the source type to the child target type
                            mappings[sourceTypeName] = (childTargetTypeName, sourceTypeName);
                        }
                    }
                }
            }
        }

        return mappings;
    }

    /// <summary>
    ///     Extracts nested wrapper mappings from the NestedWrappers parameter.
    ///     Returns a dictionary mapping source type full names to nested wrapper type information.
    /// </summary>
    public static Dictionary<string, (string childWrapperTypeName, string sourceTypeName)> ExtractNestedWrapperMappings(
        AttributeData attribute,
        Compilation compilation)
    {
        var mappings = new Dictionary<string, (string, string)>();

        var childrenArg = attribute.NamedArguments.FirstOrDefault(kvp => kvp.Key == Constants.AttributeNames.NestedWrappers);
        if (childrenArg.Value.Kind != TypedConstantKind.Error && !childrenArg.Value.IsNull)
        {
            if (childrenArg.Value.Kind == TypedConstantKind.Array)
            {
                foreach (var childValue in childrenArg.Value.Values)
                {
                    if (childValue.Value is INamedTypeSymbol childWrapperType)
                    {
                        // Find the Wrapper attribute on the child type to get its source type
                        var childWrapperAttr = childWrapperType.GetAttributes()
                            .FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == Constants.WrapperAttributeFullName);

                        if (childWrapperAttr != null && childWrapperAttr.ConstructorArguments.Length > 0)
                        {
                            if (childWrapperAttr.ConstructorArguments[0].Value is INamedTypeSymbol childSourceType)
                            {
                                var sourceTypeName = childSourceType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                                var childWrapperTypeName = childWrapperType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

                                // Map the source type to the child wrapper type
                                mappings[sourceTypeName] = (childWrapperTypeName, sourceTypeName);
                            }
                        }
                    }
                }
            }
        }

        return mappings;
    }

    /// <summary>
    ///     Gets a named argument value from the attribute, or returns the default value if not found.
    /// </summary>
    public static T GetNamedArg<T>(
        ImmutableArray<KeyValuePair<string, TypedConstant>> args,
        string name,
        T defaultValue)
        => args.FirstOrDefault(kv => kv.Key == name)
            .Value.Value is T t ? t : defaultValue;

    /// <summary>
    ///     Checks if a named argument exists in the attribute.
    /// </summary>
    public static bool HasNamedArg(
        ImmutableArray<KeyValuePair<string, TypedConstant>> args,
        string name)
        => args.Any(kv => kv.Key == name);

    /// <summary>
    ///     Extracts the excluded members list from the attribute constructor arguments.
    /// </summary>
    public static HashSet<string> ExtractExcludedMembers(AttributeData attribute)
    {
        var excludeArgIndex = GetExcludeArgumentIndex(attribute);
        if (attribute.ConstructorArguments.Length > excludeArgIndex)
        {
            var excludeArg = attribute.ConstructorArguments[excludeArgIndex];
            if (excludeArg.Kind == TypedConstantKind.Array)
            {
                return new HashSet<string>(
                    excludeArg.Values
                        .Select(v => v.Value?.ToString())
                        .Where(n => n != null)!);
            }
        }

        return new HashSet<string>();
    }

    /// <summary>
    ///     Extracts the included members list from the attribute named arguments.
    /// </summary>
    public static (HashSet<string> includedMembers, bool isIncludeMode) ExtractIncludedMembers(AttributeData attribute)
    {
        var includeArg = attribute.NamedArguments.FirstOrDefault(kvp => kvp.Key == Constants.AttributeNames.Include);
        if (includeArg.Value.Kind != TypedConstantKind.Error && !includeArg.Value.IsNull)
        {
            if (includeArg.Value.Kind == TypedConstantKind.Array)
            {
                var included = new HashSet<string>(
                    includeArg.Value.Values
                        .Select(v => v.Value?.ToString())
                        .Where(n => n != null)!);
                return (included, true);
            }
        }

        return (new HashSet<string>(), false);
    }

    /// <summary>
    ///     Extracts the configuration type name from the attribute.
    /// </summary>
    public static string? ExtractConfigurationTypeName(AttributeData attribute)
    {
        return attribute.NamedArguments
            .FirstOrDefault(kvp => kvp.Key == Constants.AttributeNames.Configuration)
            .Value.Value?
            .ToString();
    }

    /// <summary>
    ///     Extracts the BeforeMapConfiguration type name from the attribute.
    /// </summary>
    public static string? ExtractBeforeMapConfigurationTypeName(AttributeData attribute)
    {
        var arg = attribute.NamedArguments
            .FirstOrDefault(kvp => kvp.Key == Constants.AttributeNames.BeforeMapConfiguration);
        
        if (arg.Value.Value is INamedTypeSymbol typeSymbol)
        {
            return typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        }
        
        return null;
    }

    /// <summary>
    ///     Extracts the AfterMapConfiguration type name from the attribute.
    /// </summary>
    public static string? ExtractAfterMapConfigurationTypeName(AttributeData attribute)
    {
        var arg = attribute.NamedArguments
            .FirstOrDefault(kvp => kvp.Key == Constants.AttributeNames.AfterMapConfiguration);
        
        if (arg.Value.Value is INamedTypeSymbol typeSymbol)
        {
            return typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        }
        
        return null;
    }

    /// <summary>
    ///     Extracts the ConvertEnumsTo type from the attribute.
    ///     Returns "string" or "int" if specified, otherwise null.
    /// </summary>
    public static string? ExtractConvertEnumsTo(AttributeData attribute)
    {
        var arg = attribute.NamedArguments
            .FirstOrDefault(kvp => kvp.Key == Constants.AttributeNames.ConvertEnumsTo);

        if (arg.Value.Value is INamedTypeSymbol typeSymbol)
        {
            return typeSymbol.SpecialType switch
            {
                SpecialType.System_String => "string",
                SpecialType.System_Int32 => "int",
                _ => null
            };
        }

        return null;
    }

    /// <summary>
    ///     Extracts the FlattenTo types from the FlattenTo parameter.
    ///     Returns a list of fully qualified type names of flatten target types.
    /// </summary>
    public static ImmutableArray<string> ExtractFlattenToTypes(AttributeData attribute)
    {
        var flattenToArg = attribute.NamedArguments.FirstOrDefault(kvp => kvp.Key == Constants.AttributeNames.FlattenTo);
        if (flattenToArg.Value.Kind != TypedConstantKind.Error && !flattenToArg.Value.IsNull)
        {
            if (flattenToArg.Value.Kind == TypedConstantKind.Array)
            {
                var types = new List<string>();
                foreach (var typeValue in flattenToArg.Value.Values)
                {
                    if (typeValue.Value is INamedTypeSymbol flattenToType)
                    {
                        var typeName = flattenToType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                        types.Add(typeName);
                    }
                }
                return types.ToImmutableArray();
            }
        }

        return ImmutableArray<string>.Empty;
    }
}
