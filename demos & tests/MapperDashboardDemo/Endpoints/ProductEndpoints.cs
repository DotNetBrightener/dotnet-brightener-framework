using DotNetBrightener.Mapper.Mapping;
using MapperDashboardDemo.DtoTargets;
using MapperDashboardDemo.Entities;
using MapperDashboardDemo.Services;

namespace MapperDashboardDemo.Endpoints;

public static class ProductEndpoints
{
    public static void MapProductEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/products");

        group.MapGet("/", (SeedDataService data) =>
        {
            return data.Products.SelectTargets<Product, ProductDto>();
        });

        group.MapGet("/lookup", (SeedDataService data) =>
        {
            return data.Products.SelectTargets<Product, ProductLookupDto>();
        });

        group.MapGet("/query", (string? name, decimal? minPrice, SeedDataService data) =>
        {
            var query = data.Products.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(name))
                query = query.Where(p => p.Name.Contains(name, StringComparison.OrdinalIgnoreCase));

            if (minPrice.HasValue)
                query = query.Where(p => p.Price >= minPrice.Value);

            return query.SelectTargets<Product, ProductQueryDto>();
        });

        group.MapGet("/{id:int}/validated", (int id, SeedDataService data) =>
        {
            var product = data.Products.FirstOrDefault(p => p.Id == id);
            if (product is null)
                return Results.NotFound();

            return Results.Ok(product.ToTarget<Product, ProductValidatedDto>());
        });
    }
}
