using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNetBrightener.Mapper.Generators.MappingTargetGenerators;

/// <summary>
///     Generates FlattenTo methods that unpack collection properties into flattened rows.
/// </summary>
internal static class FlattenToGenerator
{
    /// <summary>
    ///     Generates FlattenTo methods for all configured flatten target types.
    /// </summary>
    public static void Generate(StringBuilder sb, MappableTargetModel model, string indent, Dictionary<string, MappableTargetModel> targetLookup)
    {
        if (model.FlattenToTypes.Length == 0) return;

        // For each flatten target type, generate a FlattenTo method
        foreach (var flattenToType in model.FlattenToTypes)
        {
            GenerateFlattenToMethod(sb, model, flattenToType, indent, targetLookup);
        }
    }

    private static void GenerateFlattenToMethod(StringBuilder sb, MappableTargetModel model, string flattenToType, string indent, Dictionary<string, MappableTargetModel> targetLookup)
    {
        // Extract the simple name from the fully qualified type name
        var flattenToTypeName = ExtractSimpleName(flattenToType);

        sb.AppendLine();
        sb.AppendLine($"{indent}/// <summary>");
        sb.AppendLine($"{indent}/// Flattens collection properties into multiple {flattenToTypeName} rows,");
        sb.AppendLine($"{indent}/// combining this target's properties with each collection item.");
        sb.AppendLine($"{indent}/// </summary>");
        sb.AppendLine($"{indent}/// <returns>A list of {flattenToTypeName} instances, one per collection item.</returns>");
        sb.AppendLine($"{indent}public global::System.Collections.Generic.List<{flattenToType}> FlattenTo()");
        sb.AppendLine($"{indent}{{");

        // Find collection members in this target that are nested targets
        var collectionMembers = model.Members
            .Where(m => m.IsCollection && m.IsNestedTarget && !string.IsNullOrEmpty(m.CollectionWrapper))
            .ToList();

        if (collectionMembers.Count == 0)
        {
            // No collection properties - return empty list
            sb.AppendLine($"{indent}    return new global::System.Collections.Generic.List<{flattenToType}>();");
        }
        else
        {
            // Use the first nested target collection property
            var collectionMember = collectionMembers[0];

            sb.AppendLine($"{indent}    if ({collectionMember.Name} == null)");
            sb.AppendLine($"{indent}    {{");
            sb.AppendLine($"{indent}        return new global::System.Collections.Generic.List<{flattenToType}>();");
            sb.AppendLine($"{indent}    }}");
            sb.AppendLine();
            sb.AppendLine($"{indent}    return {collectionMember.Name}.Select(item => new {flattenToType}");
            sb.AppendLine($"{indent}    {{");

            // Generate property mappings - copy all non-collection properties from the parent
            var nonCollectionMembers = model.Members
                .Where(m => !m.IsCollection && !m.IsNestedTarget)
                .ToList();

            foreach (var member in nonCollectionMembers)
            {
                sb.AppendLine($"{indent}        {member.Name} = {member.Name},");
            }

            // Now try to map properties from the collection item
            // We need to look up the nested target type to see what properties it has
            var nestedTargetTypeName = ExtractNestedTypeName(collectionMember.TypeName);

            // Try to find the nested target in the lookup - try multiple key variations
            MappableTargetModel? nestedTarget = null;
            if (!string.IsNullOrEmpty(nestedTargetTypeName))
            {
                nestedTarget = FindTargetModel(nestedTargetTypeName, targetLookup);
            }

            if (nestedTarget != null)
            {
                // Map properties from the nested target using SmartLeaf-style collision detection
                var collectionPropertyName = collectionMember.Name;

                // First pass: Identify which leaf property names appear multiple times
                var leafNameCounts = new Dictionary<string, int>();
                CollectLeafNames(
                    nestedTarget,
                    targetLookup,
                    nonCollectionMembers,
                    new List<string> { collectionPropertyName },
                    leafNameCounts,
                    maxDepth: 5);

                // Build set of colliding names (names that appear more than once)
                var collidingLeafNames = new HashSet<string>(
                    leafNameCounts.Where(kvp => kvp.Value > 1).Select(kvp => kvp.Key));

                // Second pass: Recursively collect all scalar properties with smart naming
                CollectNestedProperties(
                    sb,
                    nestedTarget,
                    targetLookup,
                    nonCollectionMembers,
                    "item",
                    new List<string> { collectionPropertyName },
                    collidingLeafNames,
                    indent,
                    maxDepth: 5);
            }

            // Close the object initializer
            sb.AppendLine($"{indent}    }}).ToList();");
        }

        sb.AppendLine($"{indent}}}");
    }

    private static MappableTargetModel? FindTargetModel(string typeName, Dictionary<string, MappableTargetModel> targetLookup)
    {
        if (string.IsNullOrEmpty(typeName)) return null;

        // Try the exact name first
        if (targetLookup.TryGetValue(typeName, out var target))
        {
            return target;
        }

        // Try just the simple name
        var simpleName = ExtractSimpleName(typeName);
        if (targetLookup.TryGetValue(simpleName, out target))
        {
            return target;
        }

        // Try without global:: prefix
        var withoutGlobal = typeName.Replace("global::", "");
        if (targetLookup.TryGetValue(withoutGlobal, out target))
        {
            return target;
        }

        return null;
    }

    private static void CollectLeafNames(
        MappableTargetModel mappable,
        Dictionary<string, MappableTargetModel> targetLookup,
        List<MappableTargetMember> parentMembers,
        List<string> pathSegments,
        Dictionary<string, int> leafNameCounts,
        int maxDepth,
        int currentDepth = 0)
    {
        if (currentDepth >= maxDepth)
        {
            return;
        }

        foreach (var member in mappable.Members)
        {
            if (member.IsCollection)
            {
                continue;
            }

            if (member.IsNestedTarget)
            {
                // Recurse into nested mappable
                var nestedTargetTypeName = member.TypeName?.Replace("?", "").Trim();
                if (!string.IsNullOrEmpty(nestedTargetTypeName))
                {
                    var nestedTarget = FindTargetModel(nestedTargetTypeName, targetLookup);
                    if (nestedTarget != null)
                    {
                        var newPathSegments = new List<string>(pathSegments) { member.Name };
                        CollectLeafNames(
                            nestedTarget,
                            targetLookup,
                            parentMembers,
                            newPathSegments,
                            leafNameCounts,
                            maxDepth,
                            currentDepth + 1);
                    }
                }
            }
            else if (IsScalarType(member.TypeName))
            {
                // This is a scalar leaf property
                var leafName = member.Name;

                // Always count the occurrence, even if it collides with parent
                // Parent collisions need to be tracked so we can prefix them
                if (leafNameCounts.ContainsKey(leafName))
                {
                    leafNameCounts[leafName]++;
                }
                else
                {
                    // Initialize count
                    // If it also exists in parent, immediately mark as collision
                    int initialCount = parentMembers.Any(pm => pm.Name == leafName) ? 2 : 1;
                    leafNameCounts[leafName] = initialCount;
                }
            }
        }
    }

    private static void CollectNestedProperties(
        StringBuilder sb,
        MappableTargetModel mappable,
        Dictionary<string, MappableTargetModel> targetLookup,
        List<MappableTargetMember> parentMembers,
        string navigationPath,
        List<string> pathSegments,
        HashSet<string> collidingLeafNames,
        string indent,
        int maxDepth,
        int currentDepth = 0)
    {
        if (currentDepth >= maxDepth)
        {
            // Prevent infinite recursion in circular reference scenarios
            return;
        }

        foreach (var member in mappable.Members)
        {
            // Skip collection properties - we only flatten scalar values
            if (member.IsCollection)
            {
                continue;
            }

            if (member.IsNestedTarget)
            {
                // This is a nested mappable - recurse into it to access its properties
                var nestedTargetTypeName = member.TypeName?.Replace("?", "").Trim();

                if (!string.IsNullOrEmpty(nestedTargetTypeName))
                {
                    var nestedTarget = FindTargetModel(nestedTargetTypeName, targetLookup);
                    if (nestedTarget != null)
                    {
                        var newNavigationPath = $"{navigationPath}.{member.Name}";

                        // Add this member to the path segments for SmartLeaf naming
                        var newPathSegments = new List<string>(pathSegments) { member.Name };

                        // Recursively collect properties from this nested mappable
                        CollectNestedProperties(
                            sb,
                            nestedTarget,
                            targetLookup,
                            parentMembers,
                            newNavigationPath,
                            newPathSegments,
                            collidingLeafNames,
                            indent,
                            maxDepth,
                            currentDepth + 1);
                    }
                }
            }
            else
            {
                // Check if this is actually a scalar property or a navigation property
                // Navigation properties that aren't configured as nested targets should be skipped
                if (!IsScalarType(member.TypeName))
                {
                    // This is likely a navigation property (reference type) that's not configured as a nested mappable
                    // Skip it - the user needs to explicitly configure it as a nested mappable if they want it included
                    continue;
                }

                // This is a scalar property - add it to the flattened output
                var leafName = member.Name;
                var navigationExpression = $"{navigationPath}.{leafName}";

                // Skip Id properties to avoid collision with parent Id
                if (leafName.Equals("Id", System.StringComparison.Ordinal) &&
                    parentMembers.Any(pm => pm.Name == leafName))
                {
                    // Skip nested Id - it would conflict with parent Id
                    // User can manually add a prefixed Id property if they need it
                    continue;
                }

                // Use SmartLeaf naming strategy for all properties
                var propertyName = GenerateSmartLeafName(pathSegments, leafName, collidingLeafNames);
                sb.AppendLine($"{indent}        {propertyName} = {navigationExpression},");
            }
        }
    }

    private static string GenerateSmartLeafName(List<string> pathSegments, string leafName, HashSet<string> collidingLeafNames)
    {
        // If this leaf name collides with another, use parent + leaf
        if (collidingLeafNames.Contains(leafName) && pathSegments.Count >= 1)
        {
            // Use immediate parent + leaf name
            var parentName = pathSegments[pathSegments.Count - 1];
            return parentName + leafName;
        }

        // No collision, use leaf only
        return leafName;
    }

    private static string ExtractSimpleName(string fullyQualifiedName)
    {
        // Remove global:: prefix if present
        var name = fullyQualifiedName.Replace("global::", "");
        
        // Get the last part after the last dot
        var lastDot = name.LastIndexOf('.');
        if (lastDot >= 0)
        {
            name = name.Substring(lastDot + 1);
        }

        return name;
    }

    private static string ExtractNestedTypeName(string collectionTypeName)
    {
        // Remove nullable marker
        var typeName = collectionTypeName.Replace("?", "").Trim();
        
        // Find the opening angle bracket for the generic type
        var startIndex = typeName.IndexOf('<');
        if (startIndex < 0) return string.Empty;
        
        // Find the matching closing bracket by counting brackets
        var depth = 0;
        var endIndex = -1;
        for (var i = startIndex; i < typeName.Length; i++)
        {
            if (typeName[i] == '<')
            {
                depth++;
            }
            else if (typeName[i] == '>')
            {
                depth--;
                if (depth == 0)
                {
                    endIndex = i;
                    break;
                }
            }
        }
        
        if (endIndex < 0) return string.Empty;
        
        // Extract the type between the brackets
        var innerType = typeName.Substring(startIndex + 1, endIndex - startIndex - 1).Trim();

        return innerType;
    }

    /// <summary>
    ///     Determines if a type name represents a scalar/value type that should be included in flattening.
    ///     Returns false for reference types (navigation properties) that aren't configured as nested targets.
    /// </summary>
    private static bool IsScalarType(string? typeName)
    {
        if (string.IsNullOrEmpty(typeName)) return false;

        // Remove nullable marker and global:: prefix
        var cleanType = typeName.Replace("?", "").Replace("global::", "").Trim();

        // Remove namespace qualifications to get just the type name
        var lastDot = cleanType.LastIndexOf('.');
        if (lastDot >= 0)
        {
            cleanType = cleanType.Substring(lastDot + 1);
        }

        // Check for known scalar/value types
        // Primitives
        if (cleanType == "int" || cleanType == "Int32" ||
            cleanType == "long" || cleanType == "Int64" ||
            cleanType == "short" || cleanType == "Int16" ||
            cleanType == "byte" || cleanType == "Byte" ||
            cleanType == "sbyte" || cleanType == "SByte" ||
            cleanType == "uint" || cleanType == "UInt32" ||
            cleanType == "ulong" || cleanType == "UInt64" ||
            cleanType == "ushort" || cleanType == "UInt16" ||
            cleanType == "bool" || cleanType == "Boolean" ||
            cleanType == "float" || cleanType == "Single" ||
            cleanType == "double" || cleanType == "Double" ||
            cleanType == "char" || cleanType == "Char" ||
            cleanType == "decimal" || cleanType == "Decimal")
        {
            return true;
        }

        // Common value types
        if (cleanType == "DateTime" || cleanType == "DateTimeOffset" ||
            cleanType == "TimeSpan" || cleanType == "Guid" ||
            cleanType == "DateOnly" || cleanType == "TimeOnly")
        {
            return true;
        }

        // String (reference type but treated as scalar for flattening purposes)
        if (cleanType == "string" || cleanType == "String")
        {
            return true;
        }

        // If it's a generic Nullable<T>, check the inner type
        if (cleanType.StartsWith("Nullable<") || cleanType.StartsWith("Nullable`"))
        {
            var innerType = ExtractNestedTypeName(typeName);
            return IsScalarType(innerType);
        }

        // Everything else is likely a reference type (navigation property)
        return false;
    }
}
