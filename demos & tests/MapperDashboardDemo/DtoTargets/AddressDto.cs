using DotNetBrightener.Mapper;
using MapperDashboardDemo.Entities;

namespace MapperDashboardDemo.DtoTargets;

/// <summary>
///     Demonstrates: Nested target type for Address, used by Company and Order DTOs.
/// </summary>
[MappingTarget<Address>]
public partial record AddressDto;
