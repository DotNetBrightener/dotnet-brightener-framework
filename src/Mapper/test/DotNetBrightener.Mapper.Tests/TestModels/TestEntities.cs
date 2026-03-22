namespace DotNetBrightener.Mapper.Tests.TestModels;

public class User
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string Password { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public string AddedProperty { get; set; } = string.Empty;
}

public record Tenant
{
    public Guid Id { get; set; }
}

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int CategoryId { get; set; }
    public bool IsAvailable { get; set; }
    public DateTime CreatedAt { get; set; }
    public string InternalNotes { get; set; } = string.Empty;
}

public class Employee : User
{
    public string EmployeeId { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public decimal Salary { get; set; }
    public DateTime HireDate { get; set; }
}

public class Manager : Employee
{
    public string TeamName { get; set; } = string.Empty;
    public int TeamSize { get; set; }
    public decimal Budget { get; set; }
}

public record ClassicUser(string Id, string FirstName, string LastName, string? Email);

public record ModernUser
{
    public required string Id { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public string? Email { get; set; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public string? Bio { get; set; }
    public string? PasswordHash { get; init; }
}

public record EventLog
{
    public required string Id { get; init; }
    public required string EventType { get; init; }
    public required DateTime Timestamp { get; init; }
    public string? Message { get; init; }
    public string? UserId { get; init; }
    public required string Source { get; init; }
}

public enum UserStatus
{
    Active,
    Inactive,
    Pending,
    Suspended
}

public class UserWithEnum
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public UserStatus Status { get; set; }
    public string Email { get; set; } = string.Empty;
}

public sealed class NullableTestEntity
{
    public bool Test1 { get; set; }
    public bool? Test2 { get; set; }
    public string Test3 { get; set; } = string.Empty;
    public string? Test4 { get; set; } = null;
}

// Test entity with fields for include functionality testing
public class EntityWithFields
{
    public int Id;
    public string Name = string.Empty;
    public int Age;
    public string Email { get; set; } = string.Empty;
}

public record Dummy(string Name, int Age);

public class UserForNestedTarget
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public UserAddressForNestedTarget Address { get; set; } = new();
}

public class UserAddressForNestedTarget
{
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string FormattedAddress => $"{Street}, {City}";
}

// Test entities for inherited property exclusion
public abstract class BaseEntity<TPkKey>
{
    public TPkKey Id { get; set; } = default!;
}

public class Category : BaseEntity<uint>
{
    public required string Name { get; set; }
    public string? Description { get; set; }
}

// Test entity with non-nullable reference type properties with initializers (GitHub issue)
public class UserModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public UserSettings Settings { get; set; } = new();
}

public class UserSettings
{
    public bool NotificationsEnabled { get; set; } = true;
    public string Theme { get; set; } = "light";
    public string Language { get; set; } = "en";
}

// Target for testing property initializer preservation
[MappingTarget<UserModel>()]
public partial class UserModelDto;

// Test with init only properties that have initializers
public class InitOnlyWithInitializers
{
    public string       Id        { get; init; } = Guid.NewGuid().ToString();
    public string       Name      { get; init; } = string.Empty;
    public List<string> Tags      { get; init; } = [];
    public DateTime     CreatedAt { get; init; } = DateTime.UtcNow;
}

[MappingTarget<InitOnlyWithInitializers>()]
public partial class InitOnlyWithInitializersDto;

// When a model has a single reference type property like List<string>,
// the generated record positional constructor can become ambiguous with
// the compiler-generated copy constructor
public class ModelWithListProperty
{
    public List<string> Tags { get; set; } = [];
}

// These tests verify that the ambiguity is resolved by using typed default values
// in the generated parameterless constructor

[MappingTarget<ModelWithListProperty>()]
public partial record RecordWithListDefault;

[MappingTarget<ModelWithListProperty>( GenerateParameterlessConstructor = false)]
public partial record RecordWithListNoParameterless;

[MappingTarget<ModelWithListProperty>( GenerateProjection = false)]
public partial record RecordWithListNoProjection;

// Test with multiple properties to ensure the fix doesn't break normal cases
public class ModelWithMultipleProperties
{
    public string Name { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = [];
    public int Count { get; set; }
}

[MappingTarget<ModelWithMultipleProperties>()]
public partial record RecordWithMultipleProperties;

// Test with nullable reference type property
public class ModelWithNullableList
{
    public List<string>? Tags { get; set; }
}

[MappingTarget<ModelWithNullableList>()]
public partial record RecordWithNullableList;

// When user has initialization logic in their parameterless constructor,
// the generated constructor should chain to it
public class ModelTypeForChaining
{
    public int MaxValue { get; set; }
    public string Name { get; set; } = string.Empty;
}

[MappingTarget<ModelTypeForChaining>( GenerateParameterlessConstructor = false, ChainToParameterlessConstructor = true)]
public partial class ChainedConstructorDto
{
    public int Value { get; set; }
    public bool Initialized { get; set; }

    public ChainedConstructorDto()
    {
        // Custom initialization logic that should run when mapping
        Value = 100;
        Initialized = true;
    }
}

// Test without chaining (default behavior) for comparison
[MappingTarget<ModelTypeForChaining>( GenerateParameterlessConstructor = false)]
public partial class NonChainedConstructorDto
{
    public int Value { get; set; }
    public bool Initialized { get; set; }

    public NonChainedConstructorDto()
    {
        Value = 100;
        Initialized = true;
    }
}

// Test chaining with generated parameterless constructor disabled but still using the user's
[MappingTarget<ModelTypeForChaining>( GenerateParameterlessConstructor = false, ChainToParameterlessConstructor = true, MaxDepth = 0, PreserveReferences = false)]
public partial class ChainedConstructorNoDepthDto
{
    public int Value { get; set; }
    public bool Initialized { get; set; }

    public ChainedConstructorNoDepthDto()
    {
        Value = 200;
        Initialized = true;
    }
}

// When source has a required non-nullable nested property, 
// the generated target should respect that nullability
public class UserModelWithRequiredSettings
{
    public int Id { get; set; }
    public int SettingsId { get; set; }
    public required UserSettingsModelForNested Settings { get; set; }
}

public class UserSettingsModelForNested
{
    public int Id { get; set; }
    public int StartTick { get; set; }
    public int StopTick { get; set; }
}

[MappingTarget<UserSettingsModelForNested>( nameof(UserSettingsModelForNested.Id))]
public partial class UserSettingsTarget;

// This should generate a non-nullable Settings property because:
// 1. Source property is marked as 'required'
// 2. Source property type has NotAnnotated nullable annotation (non-nullable)
[MappingTarget<UserModelWithRequiredSettings>( PreserveRequiredProperties = true, NestedTargetTypes = [typeof(UserSettingsTarget)])]
public partial class UserWithRequiredSettingsTarget
{
    public int ProcessedTicks => Settings.StopTick - Settings.StartTick;
}

// Test without required - should be nullable for safety
public class UserModelWithOptionalSettings
{
    public int Id { get; set; }
    public UserSettingsModelForNested Settings { get; set; } = new();
}

[MappingTarget<UserModelWithOptionalSettings>( NestedTargetTypes = [typeof(UserSettingsTarget)])]
public partial class UserWithOptionalSettingsTarget;

// When source has a required non-nullable collection nested property
public class TeamModelWithRequiredMembers
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public required List<UserSettingsModelForNested> Members { get; set; }
}

[MappingTarget<TeamModelWithRequiredMembers>( PreserveRequiredProperties = true, NestedTargetTypes = [typeof(UserSettingsTarget)])]
public partial class TeamWithRequiredMembersTarget;

// This entity has non-nullable string properties WITHOUT initializers
// The generated target should not trigger CS8618 warnings
public class EntityWithNonNullableProperties
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string ComputedValue => $"{Name}-{Id}";
    public string? NullableField { get; set; }
    public int NumericValue { get; set; }
}

[MappingTarget<EntityWithNonNullableProperties>()]
public partial class NonNullablePropertyTarget;

// Also test with a required property - should not get default!
[MappingTarget<EntityWithNonNullableProperties>( PreserveRequiredProperties = false)]
public partial class NonNullablePropertyTargetNoRequired;

// Test entity for GenerateCopyConstructor and GenerateEquality
public class PersonForCopyAndEquality
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int Age { get; set; }
    public DateTime? BirthDate { get; set; }
}

// Target with copy constructor
[MappingTarget<PersonForCopyAndEquality>( GenerateCopyConstructor = true)]
public partial class PersonWithCopyConstructorDto;

// Target with equality
[MappingTarget<PersonForCopyAndEquality>( GenerateEquality = true)]
public partial class PersonWithEqualityDto;

// Target with both copy constructor and equality
[MappingTarget<PersonForCopyAndEquality>( GenerateCopyConstructor = true, GenerateEquality = true)]
public partial class PersonWithCopyAndEqualityDto;

// Target with equality on a record � equality should be ignored since records already have it
[MappingTarget<PersonForCopyAndEquality>( GenerateEquality = true)]
public partial record PersonRecordWithEquality;

// Target with copy constructor on a struct
[MappingTarget<PersonForCopyAndEquality>( GenerateCopyConstructor = true, GenerateEquality = true)]
public partial struct PersonStructWithCopyAndEquality;
