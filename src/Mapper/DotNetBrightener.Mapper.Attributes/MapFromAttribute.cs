using System;

namespace DotNetBrightener.Mapper;

/// <summary>
///     Specifies that a property should be mapped from a different source property or expression.
///     This attribute allows declarative property renaming and simple transformations without
///     requiring a full IMappingConfiguration implementation.
/// </summary>
/// <remarks>
///     <para>
///         The <see cref="MapFromAttribute"/> should be used with <c>nameof()</c> for type-safe property access:
///     </para>
///     <list type="bullet">
///         <item>
///             <description>
///                 Simple property rename: <c>[MapFrom(nameof(User.FirstName))]</c> maps from source.FirstName
///             </description>
///         </item>
///         <item>
///             <description>
///                 Nested property access: <c>[MapFrom(nameof(@User.Company.Name))]</c> maps from source.Company.Name
///                 (use <c>@</c> prefix for nested paths to get compile-time validation)
///             </description>
///         </item>
///         <item>
///             <description>
///                 Expression: <c>[MapFrom(nameof(User.FirstName) + " + \" \" + " + nameof(User.LastName))]</c> for computed values
///             </description>
///         </item>
///     </list>
///     <para>
///         When used together with the <c>Configuration</c> property on <c>MappingTarget&lt;TSource&gt;</c>, the auto-generated mappings
///         (including MapFrom) are applied first, then the custom mapper is called, allowing it to override
///         any values if needed.
///     </para>
/// </remarks>
/// <example>
///     <code>
///         [MappingTarget&lt;User&gt;]
///         public partial class UserDto
///         {
///             // Simple property rename
///             [MapFrom(nameof(User.FirstName))]
///             public string Name { get; set; }
///
///             // Nested property path (use @ prefix for full path)
///             [MapFrom(nameof(@User.Company.Name))]
///             public string CompanyName { get; set; }
///
///             // Computed expression (only case where string concatenation is used)
///             [MapFrom(nameof(User.FirstName) + " + \" \" + " + nameof(User.LastName), Reversible = false)]
///             public string FullName { get; set; }
///         }
///     </code>
/// </example>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class MapFromAttribute : Attribute
{
    /// <summary>
    ///     The source property path or expression to map from.
    ///     Use nameof() for type-safe property access.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Recommended usage with nameof():
    ///     </para>
    ///     <list type="bullet">
    ///         <item>
    ///             <description>
    ///                 Simple property: <c>nameof(SourceType.PropertyName)</c>
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 Nested path: <c>nameof(@SourceType.Nested.Property)</c> (use @ prefix for nested paths)
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 Expression: <c>nameof(SourceType.FirstName) + " + \" \" + " + nameof(SourceType.LastName)</c>
    ///             </description>
    ///         </item>
    ///     </list>
    ///     <para>
    ///         When using expressions, the source variable is implicitly available. For example,
    ///         "FirstName" is equivalent to accessing source.FirstName.
    ///     </para>
    /// </remarks>
    public string Source { get; }

    /// <summary>
    ///     Whether this mapping can be reversed in the ToSource method.
    ///     Default is false (opt-in).
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Set to true when you need the mapping to be included in ToSource().
    ///         Keep as false (default) for:
    ///     </para>
    ///     <list type="bullet">
    ///         <item>
    ///             <description>
    ///                 Read-only DTOs that don't need reverse mapping
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 Computed expressions that cannot be reversed
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 Navigation property paths (e.g., Company.Name)
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 One-way mappings where reverse mapping doesn't make sense
    ///             </description>
    ///         </item>
    ///     </list>
    ///     <para>
    ///         When false, the property will not be included in the ToSource method output.
    ///     </para>
    /// </remarks>
    public bool Reversible { get; set; } = false;

    /// <summary>
    ///     Whether to include this mapping in the generated Projection expression.
    ///     Default is true.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Set to false for mappings that cannot be translated to SQL by Entity Framework Core,
    ///         such as method calls or complex expressions that require client-side evaluation.
    ///     </para>
    ///     <para>
    ///         When false, the property will not be included in the static Projection expression,
    ///         but will still be mapped in the constructor.
    ///     </para>
    /// </remarks>
    public bool IncludeInProjection { get; set; } = true;

    /// <summary>
    ///     Creates a new MapFromAttribute that maps from the specified source property or expression.
    /// </summary>
    /// <param name="source">
    ///     The source property path (using nameof()) or expression to map from.
    /// </param>
    public MapFromAttribute(string source)
    {
        Source = source ?? throw new ArgumentNullException(nameof(source));
    }
}
