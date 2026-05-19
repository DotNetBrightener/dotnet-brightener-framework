using DotNetBrightener.Mapper;

namespace DotNetBrightener.Mapper.Generators.Shared;

/// <summary>
///     Contains constant values used throughout the mapping target source generator.
///     Centralizes magic strings and default values to improve maintainability.
/// </summary>
internal static class Constants
{
    /// <summary>
    ///     The version of the mapping target generator, cached for performance.
    /// </summary>
    public static readonly string GeneratorVersion = GetGeneratorVersion();

    private static string GetGeneratorVersion()
    {
        try
        {
            return typeof(Constants).Assembly.GetName().Version?.ToString() ?? "Unknown";
        }
        catch
        {
            return "Unknown";
        }
    }

    /// <summary>
    ///     The fully qualified metadata name of the generic MappingTarget attribute.
    /// </summary>
    public static readonly string GenericMappingAttributeFullName = typeof(MappingTargetAttribute<>).FullName;

    /// <summary>
    ///     The fully qualified name of the WrapperAttribute.
    /// </summary>
    public static readonly string WrapperAttributeFullName = typeof(WrapperAttribute).FullName;

    /// <summary>
    ///     The fully qualified name of the MapFromAttribute.
    /// </summary>
    public static readonly string MapFromAttributeFullName = typeof(MapFromAttribute).FullName;

    /// <summary>
    ///     The fully qualified name of the MapWhenAttribute.
    /// </summary>
    public static readonly string MapWhenAttributeFullName = typeof(MapWhenAttribute).FullName;

    /// <summary>
    ///     The default maximum depth for nested target traversal to prevent stack overflow.
    /// </summary>
    public const int DefaultMaxDepth = 10;

    /// <summary>
    ///     The default setting for preserving object references during mapping to detect circular references.
    /// </summary>
    public const bool DefaultPreserveReferences = true;

    /// <summary>
    ///     The prefix used to specify global namespace qualification.
    /// </summary>
    public const string GlobalNamespacePrefix = "global::";

    /// <summary>
    ///     The number of spaces per indentation level in generated code.
    /// </summary>
    public const int SpacesPerIndentLevel = 4;

    /// <summary>
    ///     Standard collection wrapper type names.
    /// </summary>
    public static class CollectionWrappers
    {
        public const string List = "List";
        public const string IList = "IList";
        public const string ICollection = "ICollection";
        public const string IEnumerable = "IEnumerable";
        public const string IReadOnlyList = "IReadOnlyList";
        public const string IReadOnlyCollection = "IReadOnlyCollection";
        public const string Array = "array";
    }

    /// <summary>
    ///     Common attribute names used in mapping target generation.
    /// </summary>
    public static class AttributeNames
    {
        public const string NestedTargetTypes = "NestedTargetTypes";
        public const string NestedWrappers = "NestedWrappers";
        public const string FlattenTo = "FlattenTo";
        public const string Include = "Include";
        public const string Configuration = "Configuration";
        public const string BeforeMapConfiguration = "BeforeMapConfiguration";
        public const string AfterMapConfiguration = "AfterMapConfiguration";
        public const string IncludeFields = "IncludeFields";
        public const string GenerateConstructor = "GenerateConstructor";
        public const string GenerateParameterlessConstructor = "GenerateParameterlessConstructor";
        public const string ChainToParameterlessConstructor = "ChainToParameterlessConstructor";
        public const string GenerateProjection = "GenerateProjection";
        public const string GenerateToSource = "GenerateToSource";
        public const string PreserveInitOnlyProperties = "PreserveInitOnlyProperties";
        public const string PreserveRequiredProperties = "PreserveRequiredProperties";
        public const string NullableProperties = "NullableProperties";
        public const string CopyAttributes = "CopyAttributes";
        public const string MaxDepth = "MaxDepth";
        public const string PreserveReferences = "PreserveReferences";
        public const string UseFullName = "UseFullName";
        public const string ReadOnly = "ReadOnly";
        public const string ConvertEnumsTo = "ConvertEnumsTo";
        public const string GenerateCopyConstructor = "GenerateCopyConstructor";
        public const string GenerateEquality = "GenerateEquality";
    }
}
