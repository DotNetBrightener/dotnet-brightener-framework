namespace DotNetBrightener.Mapper.Dashboard;

/// <summary>
///     Represents information about a source type and all its associated mapping types.
/// </summary>
public sealed class MappingSourceInfo
{
    /// <summary>
    ///     Gets the source type from which mapping types are generated.
    /// </summary>
    public Type SourceType { get; }

    /// <summary>
    ///     Gets the fully qualified name of the source type.
    /// </summary>
    public string SourceTypeName => SourceType.FullName ?? SourceType.Name;

    /// <summary>
    ///     Gets the simple name of the source type.
    /// </summary>
    public string SourceTypeSimpleName => SourceType.Name;

    /// <summary>
    ///     Gets the namespace of the source type.
    /// </summary>
    public string? SourceTypeNamespace => SourceType.Namespace;

    /// <summary>
    ///     Gets the collection of mapping types generated from this source type.
    /// </summary>
    public IReadOnlyList<MappingTypeInfo> MappingTypes { get; }

    /// <summary>
    ///     Gets the collection of properties on the source type.
    /// </summary>
    public IReadOnlyList<MappingMemberInfo> SourceMembers { get; }

    /// <summary>
    ///     Creates a new instance of <see cref="MappingSourceInfo"/>.
    /// </summary>
    public MappingSourceInfo(Type sourceType, IEnumerable<MappingTypeInfo> targets, IEnumerable<MappingMemberInfo> sourceMembers)
    {
        SourceType = sourceType ?? throw new ArgumentNullException(nameof(sourceType));
        MappingTypes = targets?.ToList().AsReadOnly() ?? throw new ArgumentNullException(nameof(targets));
        SourceMembers = sourceMembers?.ToList().AsReadOnly() ?? throw new ArgumentNullException(nameof(sourceMembers));
    }
}

/// <summary>
///     Represents information about a single mapping type.
/// </summary>
public sealed class MappingTypeInfo
{
    /// <summary>
    ///     Gets the mapping type.
    /// </summary>
    public Type MappingType { get; }

    /// <summary>
    ///     Gets the fully qualified name of the target type.
    /// </summary>
    public string MappingTypeName => MappingType.FullName ?? MappingType.Name;

    /// <summary>
    ///     Gets the simple name of the target type.
    /// </summary>
    public string MappingTypeSimpleName => MappingType.Name;

    /// <summary>
    ///     Gets the namespace of the target type.
    /// </summary>
    public string? MappingTypeNamespace => MappingType.Namespace;

    /// <summary>
    ///     Gets whether this target generates a constructor from the source type.
    /// </summary>
    public bool HasConstructor { get; }

    /// <summary>
    ///     Gets whether this target has a Projection expression.
    /// </summary>
    public bool HasProjection { get; }

    /// <summary>
    ///     Gets whether this target can map back to the source type.
    /// </summary>
    public bool HasToSource { get; }

    /// <summary>
    ///     Gets the list of excluded property names.
    /// </summary>
    public IReadOnlyList<string> ExcludedProperties { get; }

    /// <summary>
    ///     Gets the list of included property names (null if using exclude mode).
    /// </summary>
    public IReadOnlyList<string>? IncludedProperties { get; }

    /// <summary>
    ///     Gets the members of this target type.
    /// </summary>
    public IReadOnlyList<MappingMemberInfo> Members { get; }

    /// <summary>
    ///     Gets the nested mapping types used by this target.
    /// </summary>
    public IReadOnlyList<Type> NestedTargetTypes { get; }

    /// <summary>
    ///     Gets the type kind (class, record, struct, record struct).
    /// </summary>
    public string TypeKind { get; }

    /// <summary>
    ///     Gets whether all properties are made nullable.
    /// </summary>
    public bool NullableProperties { get; }

    /// <summary>
    ///     Gets whether attributes are copied from source.
    /// </summary>
    public bool CopyAttributes { get; }

    /// <summary>
    ///     Gets the configuration type name if specified.
    /// </summary>
    public string? ConfigurationTypeName { get; }

    /// <summary>
    ///     Creates a new instance of <see cref="MappingTypeInfo"/>.
    /// </summary>
    public MappingTypeInfo(Type                           targetType,
                           bool                           hasConstructor,
                           bool                           hasProjection,
                           bool                           hasToSource,
                           IEnumerable<string>            excludedProperties,
                           IEnumerable<string>?           includedProperties,
                           IEnumerable<MappingMemberInfo> members,
                           IEnumerable<Type>              nestedTargets,
                           string                         typeKind,
                           bool                           nullableProperties,
                           bool                           copyAttributes,
                           string?                        configurationTypeName)
    {
        MappingType           = targetType ?? throw new ArgumentNullException(nameof(targetType));
        HasConstructor        = hasConstructor;
        HasProjection         = hasProjection;
        HasToSource           = hasToSource;
        ExcludedProperties    = excludedProperties?.ToList().AsReadOnly() ?? new List<string>().AsReadOnly();
        IncludedProperties    = includedProperties?.ToList().AsReadOnly();
        Members               = members?.ToList().AsReadOnly() ?? throw new ArgumentNullException(nameof(members));
        NestedTargetTypes     = nestedTargets?.ToList().AsReadOnly() ?? new List<Type>().AsReadOnly();
        TypeKind              = typeKind;
        NullableProperties    = nullableProperties;
        CopyAttributes        = copyAttributes;
        ConfigurationTypeName = configurationTypeName;
    }
}
