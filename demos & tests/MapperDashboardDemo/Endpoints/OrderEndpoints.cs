using DotNetBrightener.Mapper.Mapping;
using MapperDashboardDemo.DtoTargets;
using MapperDashboardDemo.Entities;
using MapperDashboardDemo.Services;

namespace MapperDashboardDemo.Endpoints;

public static class OrderEndpoints
{
    public static void MapOrderEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/orders");

        group.MapGet("/", (SeedDataService data) =>
        {
            return data.Orders.SelectTargets<Order, OrderListDto>();
        });

        group.MapGet("/{id:int}", (int id, SeedDataService data) =>
        {
            var order = data.Orders.FirstOrDefault(o => o.Id == id);
            if (order is null)
                return Results.NotFound();

            return Results.Ok(order.ToTarget<Order, OrderDto>());
        });
    }
}
