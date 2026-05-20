using DotNetBrightener.Mapper.Mapping.Configurations;

namespace DotNetBrightener.Mapper.Tests.TestModels;

// [MappingTarget<User>( "Password", "CreatedAt", GenerateToSource = true, SourceSignature = "a83684c8")]
[MappingTarget<User>("Password", "CreatedAt", GenerateToSource = true, SourceSignature = "a83684c8")]
public partial class UserDto
{
    public string FullName { get; set; } = string.Empty;
    public int Age { get; set; }
}

[MappingTarget<Product>("InternalNotes", GenerateToSource = true)]
public partial record ProductDto;

[MappingTarget<Employee>("Password", "Salary", "CreatedAt", GenerateToSource = true)]
public partial class EmployeeDto;

[MappingTarget<Manager>("Password", "Salary", "Budget", "CreatedAt", GenerateToSource = true)]
public partial class ManagerDto;

[MappingTarget<ClassicUser>(GenerateToSource = true)]
public partial record ClassicUserDto;

[MappingTarget<ModernUser>("PasswordHash", "Bio", GenerateToSource = true)]
public partial record ModernUserDto
{
    public string FullName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
}

[MappingTarget<UserWithEnum>(GenerateToSource = true)]
public partial class UserWithEnumDto;

[MappingTarget<User>("Password", "CreatedAt")]
public partial record struct UserSummary;

[MappingTarget<Product>("InternalNotes", "CreatedAt")]
public partial struct ProductSummary;

[MappingTarget<EventLog>("Source", GenerateToSource = true)]
public partial class EventLogDto;

// Include functionality test DTOs
[MappingTarget<User>(Include = ["FirstName", "LastName", "Email"], GenerateToSource = true)]
public partial class UserIncludeDto;

[MappingTarget<User>(Include = ["FirstName"])]
public partial class UserSingleIncludeDto;

[MappingTarget<User>(Include = ["DateOfBirth"])]
public partial record UserSingleObjectIncludeDto;

[MappingTarget<Tenant>()]
public partial record TenantSingleObjectIncludeDto;

[MappingTarget<Product>(Include = ["Name", "Price"])]
public partial class ProductIncludeDto;

[MappingTarget<Employee>(Include = ["FirstName", "LastName", "Department"])]
public partial class EmployeeIncludeDto;

[MappingTarget<User>(Include = ["FirstName", "LastName"])]
public partial class UserIncludeWithCustomDto
{
    public string FullName { get; set; } = string.Empty;
}

[MappingTarget<ModernUser>(Include = ["FirstName", "LastName"])]
public partial record ModernUserIncludeDto;

[MappingTarget<EntityWithFields>(Include = ["Name", "Age"], IncludeFields = true)]
public partial class EntityWithFieldsIncludeDto;

[MappingTarget<EntityWithFields>(Include = ["Email", "Name", "Age"], IncludeFields = false)]
public partial class EntityWithFieldsIncludeNoFieldsDto;

public class UserDtoWithMappingMapper : IMappingConfiguration<User, UserDtoWithMapping>
{
    public static void Map(User source, UserDtoWithMapping target)
    {
        target.FullName = $"{source.FirstName} {source.LastName}";
        target.Age = CalculateAge(source.DateOfBirth);
    }

    private static int CalculateAge(DateTime birthDate)
    {
        var today = DateTime.Today;
        var age = today.Year - birthDate.Year;
        if (birthDate.Date > today.AddYears(-age)) age--;
        return age;
    }
}

[MappingTarget<User>("Password", "CreatedAt", Configuration = typeof(UserDtoWithMappingMapper))]
public partial class UserDtoWithMapping 
{
    public string FullName { get; set; } = string.Empty;
    public int Age { get; set; }
}

// Async mapping test classes - using existing UserDto
public class UserDtoAsyncMapper : IMappingConfigurationAsync<User, UserDto>
{
    public static async Task MapAsync(User source, UserDto target, CancellationToken cancellationToken = default)
    {
        // Simulate async work
        await Task.Delay(10, cancellationToken);
        
        // Set the custom properties that UserDto has
        target.FullName = $"{source.FirstName} {source.LastName}";
        target.Age = CalculateAge(source.DateOfBirth);
    }

    private static int CalculateAge(DateTime birthDate)
    {
        var today = DateTime.Today;
        var age = today.Year - birthDate.Year;
        if (birthDate.Date > today.AddYears(-age)) age--;
        return age;
    }
}

[MappingTarget<User>(nameof(User.Password), nameof(User.CreatedAt))]
public partial class UserAsyncDto 
{
    public string FullName { get; set; } = string.Empty;
    public int Age { get; set; }
    public string ProfileData { get; set; } = string.Empty;
}

public class ProductDtoAsyncMapper : IMappingConfigurationAsync<Product, ProductDto>
{
    public static async Task MapAsync(Product source, ProductDto target, CancellationToken cancellationToken = default)
    {
        await Task.Delay(5, cancellationToken);
        
        // ProductDto has different properties - let's set what it actually has
        // For this simple test, we'll just ensure the basic properties are copied by the constructor
        // and we can add any additional logic here if needed
    }
}

[MappingTarget<Product>(nameof(Product.InternalNotes))]
public partial class ProductAsyncDto 
{
    public string DisplayName { get; set; } = string.Empty;
    public string FormattedPrice { get; set; } = string.Empty;
    public string Availability { get; set; } = string.Empty;
}

public class UserDtoHybridMapper : IMappingConfiguration<User, UserDto>,
                                   IMappingConfigurationAsync<User, UserDto>
{
    public static void Map(User source, UserDto target)
    {
        // Sync mapping
        target.FullName = $"{source.FirstName} {source.LastName}";
        target.Age      = CalculateAge(source.DateOfBirth);
    }

    public static async Task MapAsync(User source, UserDto target, CancellationToken cancellationToken = default)
    {
        // Async mapping - for this simple test, just add some delay
        await Task.Delay(8, cancellationToken);
        // UserDto doesn't have AsyncComputedField, so we'll just modify existing properties
        target.FullName += " (Hybrid)";
    }

    private static int CalculateAge(DateTime birthDate)
    {
        var today = DateTime.Today;
        var age   = today.Year - birthDate.Year;
        if (birthDate.Date > today.AddYears(-age)) age--;

        return age;
    }
}

[MappingTarget<User>(nameof(User.Password), nameof(User.CreatedAt))]
public partial class UserHybridDto 
{
    public string FullName { get; set; } = string.Empty;
    public int Age { get; set; }
    public string AsyncComputedField { get; set; } = string.Empty;
}

[MappingTarget<NullableTestEntity>]
public partial class NullableTestDto
{
}

// Test for GitHub issue: Source type with NO nullable properties but target has nullable user-defined property
[MappingTarget<Dummy>(exclude: [nameof(Dummy.Age)])]
public partial record DummyDto
{
    /// <summary>
    ///     User-defined nullable property
    /// </summary>
    public string? NameInUpperCase { get; init; }
}

// Test for GitHub issue #194: Nested target inside another target
[MappingTarget<UserForNestedTarget>(Include = [
    nameof(UserForNestedTarget.Id),
    nameof(UserForNestedTarget.Name),
    nameof(UserForNestedTarget.Address)
], NestedTargetTypes = [typeof(UserDetailResponse.UserAddressItem)])]
public partial class UserDetailResponse
{
    [MappingTarget<UserAddressForNestedTarget>(Include =
    [
        nameof(UserAddressForNestedTarget.FormattedAddress)
    ])]
    public partial class UserAddressItem;
}

// NullableProperties functionality test DTOs
[MappingTarget<Product>( "InternalNotes", "CreatedAt", NullableProperties = true, GenerateToSource = false)]
public partial class ProductQueryDto;

[MappingTarget<User>( "Password", "CreatedAt", NullableProperties = true, GenerateToSource = false)]
public partial record UserQueryDto;

[MappingTarget<UserWithEnum>( NullableProperties = true, GenerateToSource = false)]
public partial class UserWithEnumQueryDto;

// Test for excluding inherited property from base class
[MappingTarget<Category>( "Id")]
public partial record UpdateCategoryViewModel;

// ConvertEnumsTo functionality test DTOs
[MappingTarget<UserWithEnum>( ConvertEnumsTo = typeof(string), GenerateToSource = true)]
public partial class UserWithEnumToStringDto;

[MappingTarget<UserWithEnum>( ConvertEnumsTo = typeof(int), GenerateToSource = true)]
public partial class UserWithEnumToIntDto;

// Test with nullable enum property
public class EntityWithNullableEnum
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public UserStatus? Status { get; set; }
    public UserStatus NonNullableStatus { get; set; }
}

[MappingTarget<EntityWithNullableEnum>( ConvertEnumsTo = typeof(string), GenerateToSource = true)]
public partial class NullableEnumToStringDto;

[MappingTarget<EntityWithNullableEnum>( ConvertEnumsTo = typeof(int), GenerateToSource = true)]
public partial class NullableEnumToIntDto;

// Test ConvertEnumsTo with NullableProperties = true
[MappingTarget<UserWithEnum>( ConvertEnumsTo = typeof(string), NullableProperties = true)]
public partial class UserWithEnumToStringNullableDto;
