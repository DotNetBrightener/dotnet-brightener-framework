using DotNetBrightener.Mapper;
using MapperDashboardDemo.Entities;

namespace MapperDashboardDemo.DtoTargets;

// Complex nested targets: UserListItem + Address nested, OrderItem collection
[MappingTarget<Order>(NestedTargetTypes = [typeof(UserListItemDto), typeof(AddressDto)])]
public partial class OrderDto;

// Nested target for OrderItem (demonstrates collection item mapping)
[MappingTarget<OrderItem>]
public partial record OrderItemDto;

// Lightweight order for list views (include-only)
[MappingTarget<Order>(
    Include = [nameof(Order.Id), nameof(Order.TotalAmount), nameof(Order.Status), nameof(Order.OrderDate)]
)]
public partial record OrderListDto;
