using DotNetBrightener.Mapper;
using MapperDashboardDemo.DtoTargets.MappingConfigurations;
using MapperDashboardDemo.Entities;

namespace MapperDashboardDemo.DtoTargets;

// Feature 1: Basic mapping with exclude (record type)
// Dashboard shows: Constructor=Yes, Projection=Yes, ToSource=No, Excluded=[PasswordHash]
[MappingTarget<User>(nameof(User.PasswordHash))]
public partial record UserDto;

// Feature 7: ToSource (two-way mapping) - class type
// Dashboard shows: Constructor=Yes, Projection=Yes, ToSource=Yes
[MappingTarget<User>(nameof(User.PasswordHash), GenerateToSource = true)]
public partial class UserEditDto;

// Feature 2: Include-only mapping (pick specific properties)
// Dashboard shows: Included=[FirstName, LastName, Email], Excluded=[]
[MappingTarget<User>(Include = [nameof(User.FirstName), nameof(User.LastName), nameof(User.Email)])]
public partial record UserSummaryDto;

// Feature 3: Nested targets with record type
// Dashboard shows: NestedTargetTypes=[UserProfileDto], members include nested target properties
[MappingTarget<User>(
    nameof(User.PasswordHash),
    NestedTargetTypes = [typeof(UserProfileDto)]
)]
public partial record UserDetailDto;

// Feature 10: Configuration type (custom mapping logic with computed FullName and Age)
// Dashboard shows: ConfigurationTypeName = "UserFullNameConfig"
[MappingTarget<User>(
    nameof(User.PasswordHash),
    nameof(User.CreatedAt),
    Configuration = typeof(UserFullNameConfig)
)]
public partial class UserWithFullNameDto
{
    public string FullName { get; set; } = string.Empty;
    public int Age { get; set; }
}

// Multiple targets per source - minimal DTO for list views
[MappingTarget<User>(Include = [nameof(User.Id), nameof(User.FirstName), nameof(User.LastName)])]
public partial record UserListItemDto;

// Nested target for UserProfile (demonstrates deeply nested mapping)
[MappingTarget<UserProfile>]
public partial record UserProfileDto;
