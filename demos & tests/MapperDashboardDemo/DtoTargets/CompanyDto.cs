using DotNetBrightener.Mapper;
using MapperDashboardDemo.Entities;

namespace MapperDashboardDemo.DtoTargets;

// Feature 11: MapFrom - property renaming from source to target
// Dashboard shows: members with MappedFromProperty values
[MappingTarget<Company>(NestedTargetTypes = [typeof(AddressDto)])]
public partial class CompanyDto
{
    [MapFrom(nameof(Company.Name))]
    public string CompanyName { get; set; } = string.Empty;

    [MapFrom(nameof(Company.EmployeeCount))]
    public int TotalEmployees { get; set; }

    [MapFrom(nameof(Company.Revenue))]
    public decimal AnnualRevenue { get; set; }
}
