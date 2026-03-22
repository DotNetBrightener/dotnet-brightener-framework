using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using DotNetBrightener.Mapper.Generators.MappingTargetGenerators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DotNetBrightener.Mapper.Analyzers;

/// <summary>
///     Analyzer that validates proper usage of the [MappingTarget] attribute.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MappingTargetAttributeAnalyzer : DiagnosticAnalyzer
{
    // DNB003: Missing partial keyword
    public static readonly DiagnosticDescriptor MissingPartialKeywordRule = new(
                                                                                "DNB003",
                                                                                "Type with [MappingTarget] attribute must be declared as partial",
                                                                                "Type '{0}' is marked with [MappingTarget] but is not declared as partial",
                                                                                "Declaration",
                                                                                DiagnosticSeverity.Error,
                                                                                isEnabledByDefault: true,
                                                                                description: "Types marked with [MappingTarget] must be partial to allow the source generator to add generated members.");

    // DNB004: Invalid Exclude/Include property names
    public static readonly DiagnosticDescriptor InvalidPropertyNameRule = new(
                                                                              "DNB004",
                                                                              "Property name does not exist in source type",
                                                                              "Property '{0}' in {1} does not exist in source type '{2}'",
                                                                              "Usage",
                                                                              DiagnosticSeverity.Error,
                                                                              isEnabledByDefault: true,
                                                                              description: "Property names in Exclude or Include parameters must match properties in the source type.");

    // DNB005: Invalid source type
    public static readonly DiagnosticDescriptor InvalidSourceTypeRule = new(
                                                                            "DNB005",
                                                                            "Source type is not accessible or does not exist",
                                                                            "Source type '{0}' could not be resolved or is not accessible",
                                                                            "Usage",
                                                                            DiagnosticSeverity.Error,
                                                                            isEnabledByDefault: true,
                                                                            description: "The source type specified in the [MappingTarget] attribute must be a valid, accessible type.");

    // DNB006: Invalid Configuration type
    public static readonly DiagnosticDescriptor InvalidConfigurationTypeRule = new(
                                                                                   "DNB006",
                                                                                   "Configuration type does not implement required interface",
                                                                                   "Configuration type '{0}' must implement IMappingConfiguration or have a static Map method",
                                                                                   "Usage",
                                                                                   DiagnosticSeverity.Error,
                                                                                   isEnabledByDefault: true,
                                                                                   description: "Configuration types must implement the appropriate IMappingConfiguration interface or provide a static Map method.");

    // DNB007: Invalid NestedTargetTypes type
    public static readonly DiagnosticDescriptor InvalidNestedTargetRule = new(
                                                                             "DNB007",
                                                                             "Nested target type is not marked with [MappingTarget] attribute",
                                                                             "Type '{0}' in NestedTargetTypes must be marked with [MappingTarget] attribute",
                                                                             "Usage",
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: true,
                                                                             description: "All types specified in the NestedTargetTypes array must be marked with the [MappingTarget] attribute.");

    // DNB008: Circular reference warning
    public static readonly DiagnosticDescriptor CircularReferenceWarningRule = new(
                                                                                   "DNB008",
                                                                                   "Potential stack overflow with circular references",
                                                                                   "MaxDepth is 0 and PreserveReferences is false, which may cause stack overflow",
                                                                                   "Performance",
                                                                                   DiagnosticSeverity.Warning,
                                                                                   isEnabledByDefault: true,
                                                                                   description: "When working with nested targets, either MaxDepth or PreserveReferences should be enabled to prevent stack overflow.");

    // DNB009: Both Include and Exclude specified
    public static readonly DiagnosticDescriptor IncludeAndExcludeBothSpecifiedRule = new(
                                                                                         "DNB009",
                                                                                         "Cannot specify both Include and Exclude",
                                                                                         "Cannot specify both Include and Exclude parameters",
                                                                                         "Usage",
                                                                                         DiagnosticSeverity.Error,
                                                                                         isEnabledByDefault: true,
                                                                                         description: "The Include and Exclude parameters are mutually exclusive.");

    // DNB010: MaxDepth warning
    public static readonly DiagnosticDescriptor MaxDepthWarningRule = new(
                                                                          "DNB010",
                                                                          "MaxDepth value is unusual",
                                                                          "MaxDepth is set to {0}: {1}",
                                                                          "Performance",
                                                                          DiagnosticSeverity.Warning,
                                                                          isEnabledByDefault: true,
                                                                          description: "MaxDepth values should typically be between 1 and 10 for most scenarios.");

    // DNB023: GenerateToSource cannot be generated
    public static readonly DiagnosticDescriptor GenerateToSourceNotPossibleRule = new(
                                                                                      "DNB023",
                                                                                      "ToSource method cannot be generated",
                                                                                      "GenerateToSource is set to true, but ToSource cannot be generated because {0}",
                                                                                      "Usage",
                                                                                      DiagnosticSeverity.Warning,
                                                                                      isEnabledByDefault: true,
                                                                                      description: "ToSource method requires either a positional constructor or both an accessible parameterless constructor and accessible setters on all mapped properties.");

    // DNB022: Source signature mismatch
    public static readonly DiagnosticDescriptor SourceSignatureMismatchRule = new(
                                                                                  "DNB022",
                                                                                  "Source entity structure changed",
                                                                                  "Source entity '{0}' structure has changed. Update SourceSignature to '{1}' to acknowledge this change.",
                                                                                   "Mapper.SourceTracking",
                                                                                  DiagnosticSeverity.Warning,
                                                                                  isEnabledByDefault: true,
                                                                                  description: "The source entity's structure has changed since the SourceSignature was set. Review the changes and update the signature to acknowledge them.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
        MissingPartialKeywordRule,
        InvalidPropertyNameRule,
        InvalidSourceTypeRule,
        InvalidConfigurationTypeRule,
        InvalidNestedTargetRule,
        CircularReferenceWarningRule,
        IncludeAndExcludeBothSpecifiedRule,
        MaxDepthWarningRule,
        GenerateToSourceNotPossibleRule,
        SourceSignatureMismatchRule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(AnalyzeNamedType, SymbolKind.NamedType);
    }

    private static void AnalyzeNamedType(SymbolAnalysisContext context)
    {
        var namedType = (INamedTypeSymbol)context.Symbol;

        // Find all [MappingTarget] attributes on this type
        var mappingTargetAttributes = namedType.GetAttributes()
            .Where(AttributeParser.IsMappingTargetAttribute)
            .ToList();

        if (!mappingTargetAttributes.Any())
            return;

        // Check if type is partial
        if (!IsPartialType(namedType))
        {
            var diagnostic = Diagnostic.Create(
                MissingPartialKeywordRule,
                namedType.Locations.FirstOrDefault(),
                namedType.Name);
            context.ReportDiagnostic(diagnostic);
        }

        // Analyze each [MappingTarget] attribute
        foreach (var targetAttr in mappingTargetAttributes)
        {
            AnalyzeMappingTargetAttribute(context, namedType, targetAttr);
        }
    }

    private static void AnalyzeMappingTargetAttribute(SymbolAnalysisContext context, INamedTypeSymbol targetType, AttributeData targetAttr)
    {
        // Validate and get source type
        if (!TryGetSourceType(context, targetAttr, out var sourceType))
            return;

        // Get all public properties/fields from source type (including inherited)
        var sourceMembers = new HashSet<string>(GetAllPublicMembers(sourceType).Select(m => m.Name));

        // Extract named arguments
        var namedArgs = new TargetNamedArguments(targetAttr.NamedArguments);

        // Validate all parameters
        ValidateExcludeParameter(context, targetAttr, sourceType, sourceMembers);
        ValidateIncludeParameter(context, targetAttr, sourceType, sourceMembers, namedArgs.Include);
        ValidateConfigurationType(context, targetAttr, sourceType, targetType, namedArgs.Configuration);
        ValidateNestedTargets(context, targetAttr, namedArgs.NestedTargetTypes);
        ValidateCircularReferenceSafety(context, targetAttr, namedArgs);
        ValidateSourceSignature(context, targetAttr, sourceType, namedArgs);
        ValidateGenerateToSource(context, targetAttr, sourceType, targetType, namedArgs);
    }

    /// <summary>
    ///     Helper struct to hold extracted named arguments for cleaner parameter passing.
    /// </summary>
    private readonly struct TargetNamedArguments
    {
        public KeyValuePair<string, TypedConstant> Include { get; }
        public KeyValuePair<string, TypedConstant> Configuration { get; }
        public KeyValuePair<string, TypedConstant> NestedTargetTypes { get; }
        public KeyValuePair<string, TypedConstant> MaxDepth { get; }
        public KeyValuePair<string, TypedConstant> PreserveReferences { get; }
        public KeyValuePair<string, TypedConstant> SourceSignature { get; }
        public KeyValuePair<string, TypedConstant> IncludeFields { get; }
        public KeyValuePair<string, TypedConstant> GenerateToSource { get; }

        public TargetNamedArguments(ImmutableArray<KeyValuePair<string, TypedConstant>> namedArguments)
        {
            Include = namedArguments.FirstOrDefault(a => a.Key == "Include");
            Configuration = namedArguments.FirstOrDefault(a => a.Key == "Configuration");
            NestedTargetTypes = namedArguments.FirstOrDefault(a => a.Key == "NestedTargetTypes");
            MaxDepth = namedArguments.FirstOrDefault(a => a.Key == "MaxDepth");
            PreserveReferences = namedArguments.FirstOrDefault(a => a.Key == "PreserveReferences");
            SourceSignature = namedArguments.FirstOrDefault(a => a.Key == "SourceSignature");
            IncludeFields = namedArguments.FirstOrDefault(a => a.Key == "IncludeFields");
            GenerateToSource = namedArguments.FirstOrDefault(a => a.Key == "GenerateToSource");
        }
    }

    private static bool TryGetSourceType(SymbolAnalysisContext context, AttributeData targetAttr, out INamedTypeSymbol sourceType)
    {
        sourceType = null!;

        var namedType = AttributeParser.ExtractSourceType(targetAttr);
        if (namedType is null)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                InvalidSourceTypeRule,
                targetAttr.ApplicationSyntaxReference?.GetSyntax().GetLocation(),
                targetAttr.AttributeClass?.ToDisplayString() ?? "unknown"));
            return false;
        }

        if (namedType.TypeKind == TypeKind.Error)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                InvalidSourceTypeRule,
                targetAttr.ApplicationSyntaxReference?.GetSyntax().GetLocation(),
                namedType.ToDisplayString()));
            return false;
        }

        sourceType = namedType;
        return true;
    }

    private static void ValidateExcludeParameter(SymbolAnalysisContext context, AttributeData targetAttr, INamedTypeSymbol sourceType, HashSet<string> sourceMembers)
    {
        foreach (var propertyName in AttributeParser.ExtractExcludedMembers(targetAttr))
        {
            if (!sourceMembers.Contains(propertyName))
            {
                ReportInvalidPropertyName(context, targetAttr, propertyName, "Exclude", sourceType, sourceMembers);
            }
        }
    }

    private static void ValidateIncludeParameter(SymbolAnalysisContext context, AttributeData targetAttr, INamedTypeSymbol sourceType, HashSet<string> sourceMembers, KeyValuePair<string, TypedConstant> includeArg)
    {
        if (includeArg.Equals(default) || includeArg.Value.IsNull || includeArg.Value.Kind != TypedConstantKind.Array)
            return;

        // Check if both Include and Exclude are specified
        bool hasExclude = AttributeParser.ExtractExcludedMembers(targetAttr).Count > 0;

        if (hasExclude)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                IncludeAndExcludeBothSpecifiedRule,
                targetAttr.ApplicationSyntaxReference?.GetSyntax().GetLocation()));
        }

        foreach (var item in includeArg.Value.Values)
        {
            if (item.Value is string propertyName && !string.IsNullOrEmpty(propertyName) && !sourceMembers.Contains(propertyName))
            {
                ReportInvalidPropertyName(context, targetAttr, propertyName, "Include", sourceType, sourceMembers);
            }
        }
    }

    private static void ValidateConfigurationType(SymbolAnalysisContext context, AttributeData targetAttr, INamedTypeSymbol sourceType, INamedTypeSymbol targetType, KeyValuePair<string, TypedConstant> configurationArg)
    {
        if (configurationArg.Equals(default) || configurationArg.Value.IsNull)
            return;

        if (configurationArg.Value.Value is INamedTypeSymbol configurationType && !ImplementsConfigurationInterface(configurationType, sourceType, targetType))
        {
            context.ReportDiagnostic(Diagnostic.Create(
                InvalidConfigurationTypeRule,
                targetAttr.ApplicationSyntaxReference?.GetSyntax().GetLocation(),
                configurationType.ToDisplayString(),
                sourceType.ToDisplayString(),
                targetType.ToDisplayString()));
        }
    }

    private static void ValidateNestedTargets(SymbolAnalysisContext context, AttributeData targetAttr, KeyValuePair<string, TypedConstant> nestedTargetsArg)
    {
        if (nestedTargetsArg.Equals(default) || nestedTargetsArg.Value.IsNull || nestedTargetsArg.Value.Kind != TypedConstantKind.Array)
            return;

        foreach (var item in nestedTargetsArg.Value.Values)
        {
            if (item.Value is INamedTypeSymbol nestedTargetType && !HasMappingTargetAttribute(nestedTargetType))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    InvalidNestedTargetRule,
                    targetAttr.ApplicationSyntaxReference?.GetSyntax().GetLocation(),
                    nestedTargetType.ToDisplayString()));
            }
        }
    }

    private static void ValidateCircularReferenceSafety(SymbolAnalysisContext context, AttributeData targetAttr, TargetNamedArguments namedArgs)
    {
        int maxDepth = 10; // default
        bool preserveReferences = true; // default

        if (!namedArgs.MaxDepth.Equals(default) && namedArgs.MaxDepth.Value.Value is int maxDepthValue)
        {
            maxDepth = maxDepthValue;

            // Validate MaxDepth range
            if (maxDepthValue < 0)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    MaxDepthWarningRule,
                    targetAttr.ApplicationSyntaxReference?.GetSyntax().GetLocation(),
                    maxDepthValue,
                    "MaxDepth cannot be negative"));
            }
            else if (maxDepthValue > 100)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    MaxDepthWarningRule,
                    targetAttr.ApplicationSyntaxReference?.GetSyntax().GetLocation(),
                    maxDepthValue,
                    "This value is unusually large and may indicate a configuration error. Consider using a value between 1 and 10"));
            }
        }

        if (!namedArgs.PreserveReferences.Equals(default) && namedArgs.PreserveReferences.Value.Value is bool preserveReferencesValue)
        {
            preserveReferences = preserveReferencesValue;
        }

        // Check for circular reference risk
        bool hasNestedTargets = !namedArgs.NestedTargetTypes.Equals(default) &&
                              !namedArgs.NestedTargetTypes.Value.IsNull &&
                              namedArgs.NestedTargetTypes.Value.Kind == TypedConstantKind.Array &&
                              namedArgs.NestedTargetTypes.Value.Values.Length > 0;

        if (hasNestedTargets && maxDepth == 0 && !preserveReferences)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                CircularReferenceWarningRule,
                targetAttr.ApplicationSyntaxReference?.GetSyntax().GetLocation()));
        }
    }

    private static void ValidateSourceSignature(SymbolAnalysisContext context, AttributeData targetAttr, INamedTypeSymbol sourceType, TargetNamedArguments namedArgs)
    {
        if (namedArgs.SourceSignature.Equals(default) || namedArgs.SourceSignature.Value.IsNull)
            return;

        if (namedArgs.SourceSignature.Value.Value is not string expectedSignature || string.IsNullOrEmpty(expectedSignature))
            return;

        // Get IncludeFields value
        bool includeFields = !namedArgs.IncludeFields.Equals(default) &&
                            namedArgs.IncludeFields.Value.Value is bool includeFieldsValue &&
                            includeFieldsValue;

        // Get exclude values from constructor
        var excludeValues = AttributeParser.ExtractExcludedMembers(targetAttr);

        // Get include value
        var includeValue = !namedArgs.Include.Equals(default) ? namedArgs.Include.Value : default;

        // Compute actual signature
        var actualSignature = ComputeSourceSignature(sourceType, excludeValues, includeValue, includeFields);

        // Compare signatures
        if (!string.Equals(expectedSignature, actualSignature, StringComparison.OrdinalIgnoreCase))
        {
            context.ReportDiagnostic(Diagnostic.Create(
                SourceSignatureMismatchRule,
                targetAttr.ApplicationSyntaxReference?.GetSyntax().GetLocation(),
                sourceType.ToDisplayString(),
                actualSignature));
        }
    }

    private static void ValidateGenerateToSource(SymbolAnalysisContext context, AttributeData targetAttr, INamedTypeSymbol sourceType, INamedTypeSymbol targetType, TargetNamedArguments namedArgs)
    {
        // Check if GenerateToSource is set to true
        if (namedArgs.GenerateToSource.Key == null || namedArgs.GenerateToSource.Value.IsNull)
            return;

        if (namedArgs.GenerateToSource.Value.Value is not bool generateToSource || !generateToSource)
            return;

        // Check if the source type has a positional constructor
        var hasPositionalConstructor = HasPositionalConstructor(sourceType);

        if (hasPositionalConstructor)
        {
            // Positional constructors can always generate ToSource
            return;
        }

        // For non-positional types, we need a parameterless constructor and accessible setters
        var hasAccessibleConstructor = HasAccessibleParameterlessConstructor(sourceType, context.Compilation.Assembly);

        if (!hasAccessibleConstructor)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                GenerateToSourceNotPossibleRule,
                targetAttr.ApplicationSyntaxReference?.GetSyntax().GetLocation(),
                "the source type does not have an accessible parameterless constructor"));
            return;
        }

        // Check if all properties that would be mapped have accessible setters
        // We need to extract the members to check this
        var excluded = ExtractExcludedMembers(targetAttr);
        var (included, isIncludeMode) = ExtractIncludedMembers(targetAttr);
        var includeFields = !namedArgs.IncludeFields.Equals(default) &&
                           namedArgs.IncludeFields.Value.Value is bool includeFieldsValue &&
                           includeFieldsValue;

        var inaccessibleProperties = GetInaccessibleSetterProperties(sourceType, excluded, included, isIncludeMode, includeFields);

        if (inaccessibleProperties.Count > 0)
        {
            var propertyList = string.Join(", ", inaccessibleProperties.Take(3).Select(p => $"'{p}'"));
            var message = inaccessibleProperties.Count > 3
                ? $"properties {propertyList} and {inaccessibleProperties.Count - 3} more do not have accessible setters"
                : $"propert{(inaccessibleProperties.Count == 1 ? "y" : "ies")} {propertyList} {(inaccessibleProperties.Count == 1 ? "does" : "do")} not have accessible setter{(inaccessibleProperties.Count == 1 ? "" : "s")}";
            
            context.ReportDiagnostic(Diagnostic.Create(
                GenerateToSourceNotPossibleRule,
                targetAttr.ApplicationSyntaxReference?.GetSyntax().GetLocation(),
                message));
        }
    }

    private static HashSet<string> ExtractExcludedMembers(AttributeData attribute)
    {
        var excluded = new HashSet<string>();
        foreach (var propertyName in AttributeParser.ExtractExcludedMembers(attribute))
        {
            excluded.Add(propertyName);
        }
        return excluded;
    }

    private static (HashSet<string> included, bool isIncludeMode) ExtractIncludedMembers(AttributeData attribute)
    {
        var included = new HashSet<string>();
        var includeArg = attribute.NamedArguments.FirstOrDefault(a => a.Key == "Include");
        
        if (!includeArg.Equals(default) && !includeArg.Value.IsNull && includeArg.Value.Kind == TypedConstantKind.Array)
        {
            foreach (var value in includeArg.Value.Values)
            {
                if (value.Value is string propertyName && !string.IsNullOrEmpty(propertyName))
                {
                    included.Add(propertyName);
                }
            }
            return (included, true);
        }
        
        return (included, false);
    }

    private static bool HasPositionalConstructor(INamedTypeSymbol sourceType)
    {
        if (sourceType.TypeKind == TypeKind.Class || sourceType.TypeKind == TypeKind.Struct)
        {
            var syntaxRef = sourceType.DeclaringSyntaxReferences.FirstOrDefault();
            if (syntaxRef != null)
            {
                var syntax = syntaxRef.GetSyntax();

                // Check for record with parameter list
                if (syntax is RecordDeclarationSyntax recordDecl && recordDecl.ParameterList != null && recordDecl.ParameterList.Parameters.Count > 0)
                {
                    return true;
                }

                // Check for regular class/struct with primary constructor (C# 12+)
                if ((syntax is ClassDeclarationSyntax classDecl && classDecl.ParameterList != null && classDecl.ParameterList.Parameters.Count > 0) ||
                    (syntax is StructDeclarationSyntax structDecl && structDecl.ParameterList != null && structDecl.ParameterList.Parameters.Count > 0))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static bool HasAccessibleParameterlessConstructor(INamedTypeSymbol sourceType, IAssemblySymbol? compilationAssembly = null)
    {
        var constructors = sourceType.InstanceConstructors;

        // Note: For classes without explicit constructors, the compiler provides an implicit
        // parameterless constructor which will be marked as IsImplicitlyDeclared = true
        foreach (var constructor in constructors)
        {
            if (constructor.Parameters.Length == 0)
            {
                if (constructor.DeclaredAccessibility == Accessibility.Public)
                    return true;

                // Internal constructors are accessible when the source type is in the same assembly
                if (constructor.DeclaredAccessibility == Accessibility.Internal &&
                    compilationAssembly != null &&
                    SymbolEqualityComparer.Default.Equals(sourceType.ContainingAssembly, compilationAssembly))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static List<string> GetInaccessibleSetterProperties(
        INamedTypeSymbol sourceType,
        HashSet<string> excluded,
        HashSet<string> included,
        bool isIncludeMode,
        bool includeFields)
    {
        var inaccessibleProperties = new List<string>();
        var members = GetAllPublicMembers(sourceType);

        foreach (var member in members)
        {
            // Apply include/exclude filters
            if (isIncludeMode)
            {
                if (!included.Contains(member.Name))
                    continue;
            }
            else
            {
                if (excluded.Contains(member.Name))
                    continue;
            }

            // Skip fields unless includeFields is true
            if (member.Kind == SymbolKind.Field && !includeFields)
                continue;

            // Check properties for accessible setters
            if (member is IPropertySymbol property)
            {
                // Check if the property has a setter and if it's accessible
                if (property.SetMethod == null)
                {
                    inaccessibleProperties.Add(property.Name);
                    continue;
                }

                // Check setter accessibility
                var setterAccessibility = property.SetMethod.DeclaredAccessibility;
                if (setterAccessibility != Accessibility.Public &&
                    setterAccessibility != Accessibility.Internal)
                {
                    inaccessibleProperties.Add(property.Name);
                }
            }
        }

        return inaccessibleProperties;
    }

    private static void ReportInvalidPropertyName(
        SymbolAnalysisContext context,
        AttributeData targetAttr,
        string propertyName,
        string parameterName,
        INamedTypeSymbol sourceType,
        HashSet<string> validProperties)
    {
        var diagnostic = Diagnostic.Create(
            InvalidPropertyNameRule,
            targetAttr.ApplicationSyntaxReference?.GetSyntax().GetLocation(),
            propertyName,
            parameterName,
            sourceType.ToDisplayString());
        context.ReportDiagnostic(diagnostic);
    }

    private static bool IsPartialType(INamedTypeSymbol type)
    {
        // A type is partial if any of its declarations has the partial modifier
        foreach (var syntaxRef in type.DeclaringSyntaxReferences)
        {
            var syntax = syntaxRef.GetSyntax();
            if (syntax is TypeDeclarationSyntax typeDecl)
            {
                if (typeDecl.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)))
                {
                    return true;
                }
            }
        }
        return false;
    }

    private static bool HasMappingTargetAttribute(ITypeSymbol type)
    {
        return type.GetAttributes().Any(attr =>
            AttributeParser.IsMappingTargetAttribute(attr));
    }

    private static bool ImplementsConfigurationInterface(INamedTypeSymbol configurationType, INamedTypeSymbol sourceType, INamedTypeSymbol targetType)
    {
        // Check for IMappingConfiguration<TSource, TTarget>
        var syncInterface = configurationType.AllInterfaces.FirstOrDefault(i =>
            i.IsGenericType &&
            i.ConstructedFrom.ToDisplayString() == "DotNetBrightener.Mapper.Mapping.Configurations.IMappingConfiguration<TSource, TTarget>" &&
            SymbolEqualityComparer.Default.Equals(i.TypeArguments[0], sourceType) &&
            SymbolEqualityComparer.Default.Equals(i.TypeArguments[1], targetType));

        if (syncInterface != null)
            return true;

        // Check for IMappingConfigurationAsync<TSource, TTarget>
        var asyncInterface = configurationType.AllInterfaces.FirstOrDefault(i =>
            i.IsGenericType &&
            i.ConstructedFrom.ToDisplayString() == "DotNetBrightener.Mapper.Mapping.Configurations.IMappingConfigurationAsync<TSource, TTarget>" &&
            SymbolEqualityComparer.Default.Equals(i.TypeArguments[0], sourceType) &&
            SymbolEqualityComparer.Default.Equals(i.TypeArguments[1], targetType));

        if (asyncInterface != null)
            return true;

        // Also check for static Map method (alternative approach without interface)
        var mapMethod = configurationType.GetMembers("Map")
            .OfType<IMethodSymbol>()
            .FirstOrDefault(m => m.IsStatic &&
                                m.Parameters.Length == 2 &&
                                SymbolEqualityComparer.Default.Equals(m.Parameters[0].Type, sourceType) &&
                                SymbolEqualityComparer.Default.Equals(m.Parameters[1].Type, targetType));

        return mapMethod != null;
    }

    private static IEnumerable<ISymbol> GetAllPublicMembers(INamedTypeSymbol type)
    {
        var visited = new HashSet<string>();
        var current = type;

        while (current != null)
        {
            foreach (var member in current.GetMembers())
            {
                if (member.DeclaredAccessibility == Accessibility.Public &&
                    !visited.Contains(member.Name) &&
                    (member.Kind == SymbolKind.Property || member.Kind == SymbolKind.Field))
                {
                    visited.Add(member.Name);
                    yield return member;
                }
            }

            current = current.BaseType;

            if (current?.SpecialType == SpecialType.System_Object)
                break;
        }
    }

    private static string ComputeSourceSignature(
        INamedTypeSymbol sourceType,
        HashSet<string> excludeSet,
        TypedConstant includeValue,
        bool includeFields)
    {
        // Get all public members from source type
        var allMembers = GetAllPublicMembers(sourceType).ToList();

        // Build include set if specified
        HashSet<string>? includeSet = null;
        if (!includeValue.IsNull && includeValue.Kind == TypedConstantKind.Array)
        {
            includeSet = new HashSet<string>();
            foreach (var item in includeValue.Values)
            {
                if (item.Value is string name && !string.IsNullOrEmpty(name))
                    includeSet.Add(name);
            }
        }

        // Filter and format members
        var filteredMembers = allMembers
            .Where(m =>
            {
                if (m.Kind == SymbolKind.Field && !includeFields)
                    return false;

                if (includeSet != null)
                    return includeSet.Contains(m.Name);

                return !excludeSet.Contains(m.Name);
            })
            .OrderBy(m => m.Name)
            .Select(m =>
            {
                var typeName = m switch
                {
                    IPropertySymbol prop => prop.Type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat),
                    IFieldSymbol field => field.Type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat),
                    _ => "unknown"
                };
                return $"{m.Name}:{typeName}";
            });

        var combined = string.Join("|", filteredMembers);

        // Compute short hash
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(combined));
        return BitConverter.ToString(hashBytes).Replace("-", "").Substring(0, 8).ToLowerInvariant();
    }
}
