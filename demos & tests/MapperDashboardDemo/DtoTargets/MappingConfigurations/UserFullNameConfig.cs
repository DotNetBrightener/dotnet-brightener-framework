using DotNetBrightener.Mapper.Mapping.Configurations;
using MapperDashboardDemo.Entities;

namespace MapperDashboardDemo.DtoTargets.MappingConfigurations;

/// <summary>
///     Custom mapping configuration that computes FullName and Age from User source.
///     Demonstrates the Configuration parameter of [MappingTarget].
/// </summary>
public class UserFullNameConfig : IMappingConfiguration<User, UserWithFullNameDto>
{
    public static void Map(User source, UserWithFullNameDto target)
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
