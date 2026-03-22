namespace DotNetBrightener.Mapper.Mapping.Configurations;

/// <summary>
///     Allows defining custom mapping logic between a source and generated target type.
/// </summary>
/// <typeparam name="TSource">
///     The source type
/// </typeparam>
/// <typeparam name="TTarget">
///     The target type
/// </typeparam>
public interface IMappingConfiguration<TSource, TTarget>
{
    static abstract void Map(TSource source, TTarget target);
}

/// <summary>
///     Instance-based interface for defining custom mapping logic with dependency injection support.
///     Use this interface when you need to inject services into your mapper.
/// </summary>
/// <typeparam name="TSource">
///     The source type
/// </typeparam>
/// <typeparam name="TTarget">
///     The target type
/// </typeparam>
public interface IMappingConfigurationInstance<TSource, TTarget>
{
    void Map(TSource source, TTarget target);
}
