using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace DotNetBrightener.Mapper.Tests.UnitTests.Core.Target;

public class CopyAttributesTests
{
    [Fact]
    public void Target_ShouldCopyAttributes_WhenCopyAttributesIsTrue()
    {
        // Arrange
        var userWithAnnotations = new UserWithDataAnnotations
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            Age = 30,
            PhoneNumber = "555-1234"
        };

        // Act
        var dto = new UserWithDataAnnotationsDto
        {
            FirstName = userWithAnnotations.FirstName,
            LastName = userWithAnnotations.LastName,
            Email = userWithAnnotations.Email,
            Age = userWithAnnotations.Age
        };

        // Assert
        var firstNameProperty = typeof(UserWithDataAnnotationsDto).GetProperty("FirstName");
        var lastNameProperty = typeof(UserWithDataAnnotationsDto).GetProperty("LastName");
        var emailProperty = typeof(UserWithDataAnnotationsDto).GetProperty("Email");
        var ageProperty = typeof(UserWithDataAnnotationsDto).GetProperty("Age");

        firstNameProperty.ShouldNotBeNull();
        firstNameProperty!.GetCustomAttribute<RequiredAttribute>().ShouldNotBeNull();
        firstNameProperty!.GetCustomAttribute<StringLengthAttribute>().ShouldNotBeNull();

        lastNameProperty.ShouldNotBeNull();
        lastNameProperty!.GetCustomAttribute<RequiredAttribute>().ShouldNotBeNull();

        emailProperty.ShouldNotBeNull();
        emailProperty!.GetCustomAttribute<RequiredAttribute>().ShouldNotBeNull();
        emailProperty!.GetCustomAttribute<EmailAddressAttribute>().ShouldNotBeNull();

        ageProperty.ShouldNotBeNull();
        ageProperty!.GetCustomAttribute<RangeAttribute>().ShouldNotBeNull();
    }

    [Fact]
    public void Target_ShouldNotCopyAttributes_WhenCopyAttributesIsFalse()
    {
        // Arrange & Act
        var dtoType = typeof(UserWithDataAnnotationsNoCopyDto);

        // Assert
        var firstNameProperty = dtoType.GetProperty("FirstName");
        var emailProperty = dtoType.GetProperty("Email");

        firstNameProperty.ShouldNotBeNull();
        firstNameProperty!.GetCustomAttributes<ValidationAttribute>().ShouldBeEmpty();

        emailProperty.ShouldNotBeNull();
        emailProperty!.GetCustomAttributes<ValidationAttribute>().ShouldBeEmpty();
    }

    [Fact]
    public void Target_ShouldPreserveAttributeParameters_WhenCopyingAttributes()
    {
        // Arrange & Act
        var firstNameProperty = typeof(UserWithDataAnnotationsDto).GetProperty("FirstName");

        // Assert
        var stringLengthAttr = firstNameProperty!.GetCustomAttribute<StringLengthAttribute>();
        stringLengthAttr.ShouldNotBeNull();
        stringLengthAttr!.MaximumLength.ShouldBe(50);
    }

    [Fact]
    public void Target_ShouldCopyRangeAttribute_WithCorrectBounds()
    {
        // Arrange
        var ageProperty = typeof(UserWithDataAnnotationsDto).GetProperty("Age");

        // Assert
        var rangeAttr = ageProperty!.GetCustomAttribute<RangeAttribute>();
        rangeAttr.ShouldNotBeNull();
        rangeAttr!.Minimum.ShouldBe(0);
        rangeAttr.Maximum.ShouldBe(150);
    }

    [Fact]
    public void Target_ShouldNotCopyCompilerGeneratedAttributes()
    {
        // Arrange
        var dtoType = typeof(UserWithDataAnnotationsDto);

        // Assert
        foreach (var property in dtoType.GetProperties())
        {
            var attributes = property.GetCustomAttributes(true);
            foreach (var attr in attributes)
            {
                var attrType = attr.GetType();
                attrType.Namespace.ShouldNotStartWith("System.Runtime.CompilerServices");
            }
        }
    }

    [Fact]
    public void Target_ShouldCopyMultipleAttributes_OnSameProperty()
    {
        // Arrange & Act
        var firstNameProperty = typeof(UserWithDataAnnotationsDto).GetProperty("FirstName");

        // Assert
        var attributes = firstNameProperty!.GetCustomAttributes<ValidationAttribute>().ToList();
        attributes.Count.ShouldBeGreaterThanOrEqualTo(2,
            "FirstName should have multiple validation attributes");
        attributes.ShouldContain(a => a is RequiredAttribute);
        attributes.ShouldContain(a => a is StringLengthAttribute);
    }

    [Fact]
    public void Target_ShouldCopyAttributes_WithNestedTargets()
    {
        var orderDtoType = typeof(ComplexOrderDto);
        var customerProperty = orderDtoType.GetProperty("Customer");
        var orderNumberProperty = orderDtoType.GetProperty("OrderNumber");
        var totalAmountProperty = orderDtoType.GetProperty("TotalAmount");

        orderNumberProperty.ShouldNotBeNull();
        orderNumberProperty!.GetCustomAttribute<RequiredAttribute>().ShouldNotBeNull();
        orderNumberProperty.GetCustomAttribute<StringLengthAttribute>()?.MaximumLength.ShouldBe(20);

        totalAmountProperty.ShouldNotBeNull();
        totalAmountProperty!.GetCustomAttribute<RangeAttribute>().ShouldNotBeNull();

        customerProperty.ShouldNotBeNull();
        customerProperty!.PropertyType.ShouldBe(typeof(ComplexCustomerDto));
    }

    [Fact]
    public void Target_ShouldCopyAttributes_OnNestedTargetProperties()
    {
        var customerDtoType = typeof(ComplexCustomerDto);
        var emailProperty = customerDtoType.GetProperty("Email");
        var fullNameProperty = customerDtoType.GetProperty("FullName");

        emailProperty.ShouldNotBeNull();
        emailProperty!.GetCustomAttribute<RequiredAttribute>().ShouldNotBeNull();
        emailProperty!.GetCustomAttribute<EmailAddressAttribute>().ShouldNotBeNull();

        fullNameProperty.ShouldNotBeNull();
        fullNameProperty!.GetCustomAttribute<StringLengthAttribute>()?.MaximumLength.ShouldBe(100);
    }

    [Fact]
    public void Target_ShouldCopyCustomAttributes()
    {
        var productType = typeof(ComplexProductDto);
        var skuProperty = productType.GetProperty("Sku");

        skuProperty.ShouldNotBeNull();
        var regexAttr = skuProperty!.GetCustomAttribute<RegularExpressionAttribute>();
        regexAttr.ShouldNotBeNull();
    }

    [Fact]
    public void Target_ShouldCopyAttributesFromDifferentNamespaces()
    {
        // This test verifies the fix for GitHub issue: Source generation adds attributes
        // like "DefaultValue" and "Column" but not their using statements.
        // Arrange & Act
        var dtoType = typeof(DatabaseTableModelDto);

        // Assert - Verify attributes from different namespaces are copied correctly
        var databaseTableIdProperty = dtoType.GetProperty("DatabaseTableID");
        var firstNameProperty = dtoType.GetProperty("FirstName");
        var systemChangeDateProperty = dtoType.GetProperty("SystemChangeDate");
        var systemChangeTypeProperty = dtoType.GetProperty("SystemChangeType");

        // Check [Key] attribute (System.ComponentModel.DataAnnotations)
        databaseTableIdProperty.ShouldNotBeNull();
        databaseTableIdProperty!.GetCustomAttribute<KeyAttribute>().ShouldNotBeNull();

        // Check [Column] attribute (System.ComponentModel.DataAnnotations.Schema)
        systemChangeDateProperty.ShouldNotBeNull();
        var columnAttr = systemChangeDateProperty!.GetCustomAttribute<ColumnAttribute>();
        columnAttr.ShouldNotBeNull();
        columnAttr!.Order.ShouldBe(500);

        // Check [DefaultValue] attribute (System.ComponentModel)
        systemChangeTypeProperty.ShouldNotBeNull();
        var defaultValueAttr = systemChangeTypeProperty!.GetCustomAttribute<DefaultValueAttribute>();
        defaultValueAttr.ShouldNotBeNull();
        defaultValueAttr!.Value.ShouldBe("I");

        // Verify all Column attributes with Order
        var systemChangeLoginProperty = dtoType.GetProperty("SystemChangeLogin");
        systemChangeLoginProperty.ShouldNotBeNull();
        var loginColumnAttr = systemChangeLoginProperty!.GetCustomAttribute<ColumnAttribute>();
        loginColumnAttr.ShouldNotBeNull();
        loginColumnAttr!.Order.ShouldBe(502);
    }

    [Fact]
    public void Target_ShouldCopyConcurrencyCheckAttribute()
    {
        // Arrange & Act
        var dtoType = typeof(DatabaseTableModelDto);
        var systemChangeDateProperty = dtoType.GetProperty("SystemChangeDate");

        // Assert - Check [ConcurrencyCheck] attribute (System.ComponentModel.DataAnnotations)
        systemChangeDateProperty.ShouldNotBeNull();
        systemChangeDateProperty!.GetCustomAttribute<ConcurrencyCheckAttribute>().ShouldNotBeNull();
    }

    [Fact]
    public void Target_ShouldCopyCustomAttributeWithEnumConstructorArgument()
    {
        // Arrange & Act
        var dtoType = typeof(DatabaseTableWithEnumAttributeDto);

        // Assert - Verify the attribute with enum was copied correctly
        var amountProperty = dtoType.GetProperty("Amount");
        amountProperty.ShouldNotBeNull();

        var defaultSortAttr = amountProperty!.GetCustomAttribute<DefaultSortAttribute>();
        defaultSortAttr.ShouldNotBeNull();
        defaultSortAttr!.Direction.ShouldBe(SortDirection.Descending);
        defaultSortAttr.SortPrecedence.ShouldBe(0);
    }

    [Fact]
    public void Target_ShouldCopyAttributeWithMultipleEnumValues()
    {
        // Additional test for enum attributes with different values
        var dtoType = typeof(DatabaseTableWithEnumAttributeDto);

        var createdAtProperty = dtoType.GetProperty("CreatedAt");
        createdAtProperty.ShouldNotBeNull();

        var defaultSortAttr = createdAtProperty!.GetCustomAttribute<DefaultSortAttribute>();
        defaultSortAttr.ShouldNotBeNull();
        defaultSortAttr!.Direction.ShouldBe(SortDirection.Ascending);
        defaultSortAttr.SortPrecedence.ShouldBe(1);
    }

    [Fact]
    public void Target_ShouldCopyAttributes_FromPartialProperty()
    {
        // Verify that attributes from a partial property defining declaration are copied
        // to the generated DTO property
        var nameProperty = typeof(SourceWithPartialPropertyDto).GetProperty("Name");
        nameProperty.ShouldNotBeNull();
        nameProperty!.GetCustomAttribute<RequiredAttribute>().ShouldNotBeNull(
            "attributes from partial property defining declarations should be copied");
    }

    [Fact]
    public void Target_ShouldNotGenerateDuplicateProperty_WhenSourceHasPartialProperty()
    {
        // Verify that Target does not create duplicate properties when the source type
        // has both a defining and implementing partial property declaration.
        var nameProperties = typeof(SourceWithPartialPropertyDto).GetProperties()
            .Where(p => p.Name == "Name")
            .ToList();
        nameProperties.Count().ShouldBe(1, "a partial property should appear only once in the DTO");
    }

    [Fact]
    public void Target_ShouldGenerateRegularProperty_ForNonPartialProperty_InPartialSourceType()
    {
        // Non-partial properties in the same source class should remain regular (non-partial).
        var ageProperty = typeof(SourceWithPartialPropertyDto).GetProperty("Age");
        ageProperty.ShouldNotBeNull();
        // Age has no [Required] on the source, so it should not appear on the DTO either
        ageProperty!.GetCustomAttribute<RequiredAttribute>().ShouldBeNull();
    }

    [Fact]
    public void Target_ShouldMapPartialPropertyValue_ThroughConstructor()
    {
        // Verify the generated constructor correctly maps values from the source's partial property.
        var source = new SourceWithPartialProperty { Name = "Alice", Age = 30 };
        var dto = new SourceWithPartialPropertyDto(source);
        dto.Name.ShouldBe("Alice");
        dto.Age.ShouldBe(30);
    }
}

// Source model with data annotations
public class UserWithDataAnnotations
{
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Range(0, 150)]
    public int Age { get; set; }

    [Phone]
    public string? PhoneNumber { get; set; }

    // This should be excluded and not appear in DTO
    public string Password { get; set; } = string.Empty;
}

// DTO with CopyAttributes = true
[MappingTarget<UserWithDataAnnotations>( "Password", "PhoneNumber", CopyAttributes = true)]
public partial class UserWithDataAnnotationsDto
{
}

// DTO with CopyAttributes = false (default)
[MappingTarget<UserWithDataAnnotations>( "Password", "PhoneNumber", "Age")]
public partial class UserWithDataAnnotationsNoCopyDto
{
}

public class ComplexCustomer
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Phone]
    public string? PhoneNumber { get; set; }
}

[MappingTarget<ComplexCustomer>( "PhoneNumber", CopyAttributes = true)]
public partial class ComplexCustomerDto
{
}

public class ComplexOrder
{
    public int Id { get; set; }

    [Required]
    [StringLength(20)]
    public string OrderNumber { get; set; } = string.Empty;

    [Range(0.01, 1000000)]
    public decimal TotalAmount { get; set; }

    public DateTime OrderDate { get; set; }

    public ComplexCustomer Customer { get; set; } = null!;

    public string? InternalNotes { get; set; }
}

[MappingTarget<ComplexOrder>( "InternalNotes", CopyAttributes = true, NestedTargetTypes = [typeof(ComplexCustomerDto)])]
public partial class ComplexOrderDto
{
}

public class ComplexProduct
{
    [Required]
    [StringLength(50)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [RegularExpression(@"^[A-Z]{3}-\d{4}$")]
    public string Sku { get; set; } = string.Empty;

    [Range(0, 10000)]
    public decimal Price { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    [Url]
    public string? ImageUrl { get; set; }

    public bool IsActive { get; set; }
}

[MappingTarget<ComplexProduct>( "IsActive", "ImageUrl", CopyAttributes = true)]
public partial class ComplexProductDto
{
}

// Source model with attributes from different namespaces (matching the reported issue)
public class DatabaseTableModel
{
    [Key]
    public long DatabaseTableID { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    [ConcurrencyCheck]
    [Column(Order = 500)]
    public DateTime? SystemChangeDate { get; set; }

    [DefaultValue("I")]
    [Column(Order = 501)]
    public string SystemChangeType { get; set; } = string.Empty;

    [Column(Order = 502)]
    public string SystemChangeLogin { get; set; } = string.Empty;
}

// DTO with CopyAttributes = true - should compile with proper using statements
[MappingTarget<DatabaseTableModel>( CopyAttributes = true)]
public partial class DatabaseTableModelDto
{
}

// Custom attribute with enum parameter
public enum SortDirection
{
    Ascending,
    Descending
}

public class DefaultSortAttribute : Attribute
{
    public SortDirection Direction { get; set; }
    public int SortPrecedence { get; set; } = 0;

    public DefaultSortAttribute(SortDirection direction, int sortPrecedence = 0)
    {
        Direction = direction;
        SortPrecedence = sortPrecedence;
    }
}

// Source model with custom attribute that has enum constructor argument
public class DatabaseTableWithEnumAttribute
{
    [Key]
    public long DatabaseTableID { get; set; }

    [DefaultSort(SortDirection.Descending, 0)]
    public decimal Amount { get; set; }

    [DefaultSort(SortDirection.Ascending, 1)]
    public DateTime CreatedAt { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;
}

// DTO with CopyAttributes = true - should compile with custom enum attribute
[MappingTarget<DatabaseTableWithEnumAttribute>( CopyAttributes = true)]
public partial class DatabaseTableWithEnumAttributeDto
{
}

public interface IDynamicDropdownAttribute
{
    Type DynamicDropdownDataProviderType { get; }

    string EmptyOptionText { get; set; }

    bool UseComboBox { get; set; }

    bool UseCheckboxMultiselect { get; set; }
}

public class DynamicDropdownAttribute<TDataProvider> : Attribute, IDynamicDropdownAttribute
    where TDataProvider : IDynamicDropdownDataProvider
{
    public Type DynamicDropdownDataProviderType
    {
        get
        {
            return typeof(TDataProvider);
        }
    }

    public string EmptyOptionText { get; set; }

    public bool UseComboBox { get; set; }

    public bool UseCheckboxMultiselect { get; set; } = false;

    public DynamicDropdownAttribute(string emptyOptionText = null, bool useComboBox = true)
    {
        EmptyOptionText = emptyOptionText;
        UseComboBox = useComboBox;
    }
}

public interface IDynamicDropdownDataProvider
{
}

public class DataSourceItem<TProperty, TListObject>
{
    public string Text { get; set; }

    public TProperty Value { get; set; }

    public TListObject Object { get; set; }

}

public abstract class DynamicDropdownDataProvider<TDataModel, TProperty, TListObject> : IDynamicDropdownDataProvider
{
    public bool AutoOrderByText { get; set; } = true;

    public abstract List<DataSourceItem<TProperty, TListObject>> UnfilteredDataList(TDataModel? model);

    public virtual List<DataSourceItem<TProperty, TListObject>> FilteredDataList(
        TDataModel? model,
        List<DataSourceItem<TProperty, TListObject>> unfilteredList)
    {
        return unfilteredList;
    }

    public string RetrieveDisplayText(List<DataSourceItem<TProperty, TListObject>> unfilteredList, PropertyInfo propertyInfo, TDataModel? context)
    {
        return string.Empty;
    }
}

public class PermissionSchemeProvider : DynamicDropdownDataProvider<DatabaseTableModel, SortDirection, SortDirection>
{
    public override List<DataSourceItem<SortDirection, SortDirection>> UnfilteredDataList(DatabaseTableModel? model)
    {
        return
        [
            new DataSourceItem<SortDirection, SortDirection>()
            {
                Object = SortDirection.Descending,
                Text   = "Descending",
                Value  = SortDirection.Descending
            },
            new DataSourceItem<SortDirection, SortDirection>()
            {
                Object = SortDirection.Ascending,
                Text   = "Test",
                Value  = SortDirection.Ascending
            }
        ];
    }
}

public class DatabaseTableWithGenericAttribute
{
    [Key]
    public long DatabaseTableID { get; set; }

    [DefaultSort(SortDirection.Descending, 0)]
    public decimal Amount { get; set; }

    [DefaultSort(SortDirection.Ascending, 1)]
    public DateTime CreatedAt { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    [DynamicDropdown<PermissionSchemeProvider>]
    public SortDirection SortDirection { get; set; }
}

// DTO with CopyAttributes = true - should compile with custom generic attribute
[MappingTarget<DatabaseTableWithGenericAttribute>( CopyAttributes = true)]
public partial class DatabaseTableWithGenericAttributeDto
{
}

public partial class SourceWithPartialProperty
{
    // Defining declaration: partial modifier + no accessor body
    [Required]
    public partial string Name { get; set; }

    public int Age { get; set; }
}

// Implementing declaration for the source type's partial property
public partial class SourceWithPartialProperty
{
    private string _sourceName = string.Empty;
    public partial string Name
    {
        get => _sourceName;
        set => _sourceName = value;
    }
}

// Target DTO — MappingTarget now generates regular (non-partial) properties from partial source properties.
// The partial modifier is NOT propagated because source generators don't chain,
// so another generator (e.g., CommunityToolkit.Mvvm) can't provide an implementing declaration. (GitHub issue #277)
// Generated:
//   [Required] public string Name { get; set; } = default!;
//   public int Age { get; set; }
//   + constructor, projection, etc.
[MappingTarget<SourceWithPartialProperty>( CopyAttributes = true)]
public partial class SourceWithPartialPropertyDto
{
}
