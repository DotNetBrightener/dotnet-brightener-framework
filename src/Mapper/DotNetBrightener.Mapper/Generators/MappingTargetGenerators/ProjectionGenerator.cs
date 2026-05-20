using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNetBrightener.Mapper.Generators.Shared;

namespace DotNetBrightener.Mapper.Generators.MappingTargetGenerators;

/// <summary>
///     Generates LINQ projection expressions for efficient database query projections.
/// </summary>
internal static class ProjectionGenerator
{
    /// <summary>
    ///     Generates the projection property for LINQ/EF Core query optimization.
    /// </summary>
    public static void GenerateProjectionProperty(
        StringBuilder sb,
        MappableTargetModel model,
        string memberIndent,
        Dictionary<string, MappableTargetModel> targetLookup)
    {
        sb.AppendLine();

        if (model.HasExistingPrimaryConstructor && model.IsRecord)
        {
            GenerateProjectionNotSupportedComment(sb, model, memberIndent);
        }
        else
        {
            GenerateProjectionDocumentation(sb, model, memberIndent);
            sb.AppendLine($"{memberIndent}public static Expression<Func<{model.SourceTypeName}, {model.Name}>> Projection =>");

            // Generate object initializer projection for EF Core compatibility
            GenerateProjectionExpression(sb, model, memberIndent, targetLookup);
        }
    }

    private static void GenerateProjectionNotSupportedComment(StringBuilder sb, MappableTargetModel model, string memberIndent)
    {
        // For records with existing primary constructors, the projection can't use the standard constructor approach
        sb.AppendLine($"{memberIndent}// Note: Projection generation is not supported for records with existing primary constructors.");
        sb.AppendLine($"{memberIndent}// You must manually create projection expressions or use the FromSource factory method.");
        sb.AppendLine($"{memberIndent}// Example: source => new {model.Name}(defaultPrimaryConstructorValue) {{ PropA = source.PropA, PropB = source.PropB }}");
    }


    private static void GenerateProjectionDocumentation(StringBuilder sb, MappableTargetModel model, string memberIndent)
    {
        // Generate projection XML documentation
        sb.AppendLine($"{memberIndent}/// <summary>");
        sb.AppendLine($"{memberIndent}/// Gets the projection expression for converting <see cref=\"{CodeGenerationHelpers.GetSimpleTypeName(model.SourceTypeName)}\"/> to <see cref=\"{model.Name}\"/>.");
        sb.AppendLine($"{memberIndent}/// Use this for LINQ and Entity Framework query projections.");
        sb.AppendLine($"{memberIndent}/// </summary>");
        sb.AppendLine($"{memberIndent}/// <value>An expression tree that can be used in LINQ queries for efficient database projections.</value>");
        sb.AppendLine($"{memberIndent}/// <example>");
        sb.AppendLine($"{memberIndent}/// <code>");
        sb.AppendLine($"{memberIndent}/// var dtos = context.{CodeGenerationHelpers.GetSimpleTypeName(model.SourceTypeName)}s");
        sb.AppendLine($"{memberIndent}///     .Where(x => x.IsActive)");
        sb.AppendLine($"{memberIndent}///     .Select({model.Name}.Projection)");
        sb.AppendLine($"{memberIndent}///     .ToList();");
        sb.AppendLine($"{memberIndent}/// </code>");
        sb.AppendLine($"{memberIndent}/// </example>");
    }

    /// <summary>
    ///     Generates the projection expression body using object initializer syntax for EF Core compatibility.
    ///     This allows EF Core to automatically include navigation properties without requiring explicit .Include() calls.
    ///     For positional records without a parameterless constructor, uses constructor invocation syntax instead.
    /// </summary>
    private static void GenerateProjectionExpression(
        StringBuilder sb,
        MappableTargetModel model,
        string baseIndent,
        Dictionary<string, MappableTargetModel> targetLookup)
    {
        var indent = baseIndent + "    ";
        
        // Check if this is a positional record without a parameterless constructor
        // In this case, we need to use constructor syntax instead of object initializer
        var isPositionalWithoutParameterless = model.IsRecord && 
                                                !model.HasExistingPrimaryConstructor && 
                                                !model.GenerateParameterlessConstructor;
        
        if (isPositionalWithoutParameterless)
        {
            // Use constructor invocation syntax for positional records
            GeneratePositionalRecordProjection(sb, model, indent, targetLookup);
        }
        else
        {
            // Use object initializer syntax (standard approach)
            GenerateObjectInitializerProjection(sb, model, indent, targetLookup);
        }
    }

    /// <summary>
    ///     Generates projection using constructor invocation syntax for positional records.
    /// </summary>
    private static void GeneratePositionalRecordProjection(
        StringBuilder sb,
        MappableTargetModel model,
        string indent,
        Dictionary<string, MappableTargetModel> targetLookup)
    {
        var visitedTypes = new HashSet<string> { model.Name };
        var includedMembers = model.Members.Where(m => m.MapFromIncludeInProjection).ToArray();
        
        sb.Append($"{indent}source => new {model.Name}(");
        
        for (int i = 0; i < includedMembers.Length; i++)
        {
            var member = includedMembers[i];
            var projectionValue = GetProjectionValueExpression(member, "source", indent, targetLookup, visitedTypes, 0, model.MaxDepth);
            sb.Append(projectionValue);
            
            if (i < includedMembers.Length - 1)
                sb.Append(", ");
        }
        
        sb.AppendLine(");");
    }

    /// <summary>
    ///     Generates projection using object initializer syntax (standard approach).
    /// </summary>
    private static void GenerateObjectInitializerProjection(
        StringBuilder sb,
        MappableTargetModel model,
        string indent,
        Dictionary<string, MappableTargetModel> targetLookup)
    {
        sb.AppendLine($"{indent}source => new {model.Name}");
        sb.AppendLine($"{indent}{{");

        var members = model.Members;

        // Track which target types we're currently processing to detect circular references
        var visitedTypes = new HashSet<string> { model.Name };

        // Pre-filter included members to avoid O(n²) comma placement check
        var includedMembers = members.Where(m => m.MapFromIncludeInProjection).ToArray();
        var includedCount = includedMembers.Length;

        for (int i = 0; i < includedCount; i++)
        {
            var member = includedMembers[i];
            var memberIndent = indent + "    ";

            // Generate the property assignment
            var projectionValue = GetProjectionValueExpression(member, "source", memberIndent, targetLookup, visitedTypes, 0, model.MaxDepth);
            sb.Append($"{memberIndent}{member.Name} = {projectionValue}");

            // Add comma if not the last member
            if (i < includedCount - 1)
                sb.Append(",");
            sb.AppendLine();
        }

        sb.AppendLine($"{indent}}};");
    }

    /// <summary>
    ///     Gets the projection expression for a member that's compatible with EF Core query translation.
    ///     For nested targets, generates nested object initializers instead of constructor calls.
    /// </summary>
    private static string GetProjectionValueExpression(
        MappableTargetMember member,
        string sourceVariableName,
        string indent,
        Dictionary<string, MappableTargetModel> targetLookup,
        HashSet<string> visitedTypes,
        int currentDepth = 0,
        int maxDepth = 0)
    {
        // Check if the member type is nullable
        bool isNullable = member.TypeName.Contains("?");

        if (member.IsNestedTarget && member.IsCollection)
        {
            return BuildCollectionProjection(member, sourceVariableName, isNullable, targetLookup, visitedTypes, currentDepth, maxDepth);
        }
        else if (member.IsNestedTarget)
        {
            return BuildSingleNestedProjection(member, sourceVariableName, isNullable, indent, targetLookup, visitedTypes, currentDepth, maxDepth);
        }

        // Check if this is a MapFrom expression (contains operators or spaces)
        string valueExpression;
        if (member.MapFromSource != null && IsExpression(member.MapFromSource))
        {
            valueExpression = TransformExpression(member.MapFromSource, sourceVariableName);
        }
        else if (member.MapFromSource != null)
        {
            // Use the full MapFromSource path for nested property paths (e.g., "Company.Address")
            valueExpression = $"{sourceVariableName}.{member.MapFromSource}";
        }
        else
        {
            // Regular property - direct assignment using SourcePropertyName
            valueExpression = $"{sourceVariableName}.{member.SourcePropertyName}";
        }

        // Apply enum conversion if this member was converted from an enum type
        if (member.IsEnumConversion && member.OriginalEnumTypeName != null)
        {
            valueExpression = ApplyEnumProjectionConversion(valueExpression, member);
        }

        // Apply MapWhen conditions if present and IncludeInProjection is true
        if (member.MapWhenConditions.Count > 0 && member.MapWhenIncludeInProjection)
        {
            valueExpression = WrapWithMapWhenCondition(member, valueExpression, sourceVariableName);
        }

        return valueExpression;
    }

    private static string BuildCollectionProjection(
        MappableTargetMember member,
        string sourceVariableName,
        bool isNullable,
        Dictionary<string, MappableTargetModel> targetLookup,
        HashSet<string> visitedTypes,
        int currentDepth,
        int maxDepth)
    {
        // Check if we've reached max depth during code generation
        // Note: maxDepth of 0 means unlimited
        if (maxDepth > 0 && currentDepth + 1 > maxDepth)
        {
            return "null";
        }

        // Use SourcePropertyName for accessing the source property (supports MapFrom)
        var sourcePropName = member.SourcePropertyName;

        // For collection nested targets, use Select with nested projection
        var elementTypeName = ExpressionBuilder.ExtractElementTypeFromCollectionTypeName(member.TypeName);
        var nonNullableElementType = elementTypeName.TrimEnd('?');

        var collectionProjection = GenerateNestedCollectionProjection(
            $"{sourceVariableName}.{sourcePropName}",
            nonNullableElementType,
            member.NestedTargetSourceTypeName!,
            member.CollectionWrapper!,
            targetLookup,
            visitedTypes,
            currentDepth + 1,
            maxDepth);

        if (isNullable)
        {
            return $"{sourceVariableName}.{sourcePropName} != null ? {collectionProjection} : null";
        }

        return collectionProjection;
    }

    private static string BuildSingleNestedProjection(
        MappableTargetMember member,
        string sourceVariableName,
        bool isNullable,
        string indent,
        Dictionary<string, MappableTargetModel> targetLookup,
        HashSet<string> visitedTypes,
        int currentDepth,
        int maxDepth)
    {
        // Check if we've reached max depth during code generation
        // Note: maxDepth of 0 means unlimited
        if (maxDepth > 0 && currentDepth + 1 > maxDepth)
        {
            return "null";
        }

        // Use SourcePropertyName for accessing the source property (supports MapFrom)
        var sourcePropName = member.SourcePropertyName;

        // For single nested targets, inline expand the nested target's members
        var nonNullableTypeName = member.TypeName.TrimEnd('?');
        var nestedSourceExpression = $"{sourceVariableName}.{sourcePropName}";

        // Extract simple type name for circular reference check
        var simpleTypeName = nonNullableTypeName.Replace(Shared.GeneratorUtilities.GlobalPrefix, "").Split('.', ':').Last();

        // Check for circular reference - if we're already processing this type, use constructor
        if (visitedTypes.Contains(simpleTypeName))
        {
            // Circular reference detected - use constructor call to prevent infinite expansion
            var nestedProjection = $"new {nonNullableTypeName}({nestedSourceExpression})";

            if (isNullable)
            {
                return $"{nestedSourceExpression} != null ? {nestedProjection} : null";
            }
            return nestedProjection;
        }

        // Try to look up the nested target model
        var nestedTargetModel = FindNestedTargetModel(nonNullableTypeName, targetLookup);

        string nestedProjectionResult;
        if (nestedTargetModel != null)
        {
            // Add this type to visited set before recursing
            visitedTypes.Add(simpleTypeName);
            try
            {
                // Recursively inline the nested target's members
                nestedProjectionResult = GenerateInlineNestedTargetInitializer(
                    nestedTargetModel,
                    nestedSourceExpression,
                    nonNullableTypeName,
                    indent,
                    targetLookup,
                    visitedTypes,
                    currentDepth + 1,
                    maxDepth);
            }
            finally
            {
                // Remove from visited set after recursion completes
                visitedTypes.Remove(simpleTypeName);
            }
        }
        else
        {
            // Fallback to constructor call if we can't find the nested target model
            nestedProjectionResult = $"new {nonNullableTypeName}({nestedSourceExpression})";
        }

        if (isNullable)
        {
            return $"{nestedSourceExpression} != null ? {nestedProjectionResult} : null";
        }

        return nestedProjectionResult;
    }

    /// <summary>
    ///     Generates an inline object initializer for a nested mappable, recursively expanding all members.
    /// </summary>
    private static string GenerateInlineNestedTargetInitializer(
        MappableTargetModel nestedMappableModel,
        string sourceExpression,
        string targetTypeName,
        string indent,
        Dictionary<string, MappableTargetModel> targetLookup,
        HashSet<string> visitedTypes,
        int currentDepth = 0,
        int maxDepth = 0)
    {
        var sb = new StringBuilder();
        sb.Append($"new {targetTypeName} {{ ");

        var members = nestedMappableModel.Members;
        for (int i = 0; i < members.Length; i++)
        {
            var member = members[i];
            var projectionValue = GetProjectionValueExpression(member, sourceExpression, indent, targetLookup, visitedTypes, currentDepth, maxDepth);
            sb.Append($"{member.Name} = {projectionValue}");

            if (i < members.Length - 1)
            {
                sb.Append(", ");
            }
        }

        sb.Append(" }");
        return sb.ToString();
    }

    /// <summary>
    ///     Generates a collection projection expression for nested targets.
    /// </summary>
        private static string GenerateNestedCollectionProjection(
        string sourceCollectionExpression,
        string elementTargetTypeName,
        string elementSourceTypeName,
        string collectionWrapper,
        Dictionary<string, MappableTargetModel> targetLookup,
        HashSet<string> visitedTypes,
        int currentDepth = 0,
        int maxDepth = 0)
    {
        // Extract simple type name for circular reference check
        var simpleTypeName = elementTargetTypeName.Replace(Shared.GeneratorUtilities.GlobalPrefix, "").Split('.', ':').Last();

        // Check for circular reference
        if (visitedTypes.Contains(simpleTypeName))
        {
            // Circular reference detected - use constructor call
            var circularProjection = $"{sourceCollectionExpression}.Select(x => new {elementTargetTypeName}(x))";
            return collectionWrapper switch
            {
                Constants.CollectionWrappers.Array => $"{circularProjection}.ToArray()",
                Constants.CollectionWrappers.IEnumerable => circularProjection,
                _ => $"{circularProjection}.ToList()"
            };
        }

        // Try to find the nested target model to inline expand it
        var nestedTargetModel = FindNestedTargetModel(elementTargetTypeName, targetLookup);

        string projection;
        if (nestedTargetModel != null)
        {
            // Add this type to visited set before recursing
            visitedTypes.Add(simpleTypeName);
            try
            {
                // Inline expand the nested target
                var inlineInitializer = GenerateInlineNestedTargetInitializer(
                    nestedTargetModel, "x", elementTargetTypeName, "", targetLookup, visitedTypes, currentDepth, maxDepth);
                projection = $"{sourceCollectionExpression}.Select(x => {inlineInitializer})";
            }
            finally
            {
                // Remove from visited set after recursion completes
                visitedTypes.Remove(simpleTypeName);
            }
        }
        else
        {
            // Fallback to constructor call
            projection = $"{sourceCollectionExpression}.Select(x => new {elementTargetTypeName}(x))";
        }

        return collectionWrapper switch
        {
            Constants.CollectionWrappers.Array => $"{projection}.ToArray()",
            Constants.CollectionWrappers.IEnumerable => projection,
            _ => $"{projection}.ToList()"
        };
    }

    private static MappableTargetModel? FindNestedTargetModel(string typeName, Dictionary<string, MappableTargetModel> targetLookup)
    {
        // Strip "global::" prefix and extract simple name
        var lookupName = typeName
            .Replace(Shared.GeneratorUtilities.GlobalPrefix, "")
            .Split('.', ':')
            .Last();

        // First try exact match with the lookup name
        if (targetLookup.TryGetValue(lookupName, out var nestedTargetModel))
        {
            return nestedTargetModel;
        }

        // Try matching by simple name or full name
        foreach (var kvp in targetLookup)
        {
            if (kvp.Key == lookupName ||
                kvp.Value.Name == lookupName ||
                kvp.Key.EndsWith("." + lookupName))
            {
                return kvp.Value;
            }
        }

        return null;
    }

    // Expression parsing methods delegated to shared ExpressionHelper
    private static bool IsExpression(string source) => ExpressionHelper.IsExpression(source);
    private static string TransformExpression(string expression, string sourceVariableName) => ExpressionHelper.TransformExpression(expression, sourceVariableName);

    /// <summary>
    ///     Applies enum-to-target-type conversion for projection expressions (EF Core compatible).
    ///     Uses expression-tree-compatible patterns.
    /// </summary>
    private static string ApplyEnumProjectionConversion(string valueExpression, MappableTargetMember member)
    {
        bool isNullableEnum = member.SourceMemberTypeName?.Contains("?") ?? false;

        if (member.TypeName.TrimEnd('?') == "string")
        {
            // For EF Core projections, .ToString() on enums translates to SQL
            if (isNullableEnum)
            {
                return $"{valueExpression} != null ? {valueExpression}.Value.ToString() : null";
            }
            return $"{valueExpression}.ToString()";
        }
        else if (member.TypeName.TrimEnd('?') == "int")
        {
            // Cast enum to int - EF Core supports this in projections
            if (isNullableEnum)
            {
                return $"(int?){valueExpression}";
            }
            return $"(int){valueExpression}";
        }

        return valueExpression;
    }

    /// <summary>
    ///     Wraps a value expression with MapWhen condition(s), generating a ternary expression.
    /// </summary>
    private static string WrapWithMapWhenCondition(MappableTargetMember member, string valueExpression, string sourceVariableName)
    {
        // Combine multiple conditions with &&
        var combinedCondition = string.Join(" && ", member.MapWhenConditions.Select(c =>
            $"({TransformExpression(c, sourceVariableName)})"));

        // Determine the default value
        var defaultValue = member.MapWhenDefault ?? Shared.GeneratorUtilities.GetDefaultValueForType(member.TypeName);

        return $"{combinedCondition} ? {valueExpression} : {defaultValue}";
    }

}
