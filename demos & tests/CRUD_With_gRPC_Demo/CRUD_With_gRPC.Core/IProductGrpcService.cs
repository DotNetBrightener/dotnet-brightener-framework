using System.Linq.Expressions;
using Bogus;
using DotNetBrightener.Framework.Exceptions;
using DotNetBrightener.gRPC;
using DotNetBrightener.GenericCRUD.Extensions;
using DotNetBrightener.GenericCRUD.Models;

namespace CRUD_With_gRPC.Core;

public class Product
{
    public long Id { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public decimal Price { get; set; }
}

public class ProductFilterQuery : BaseQueryModel
{
    public Dictionary<string, string> Filters
    {
        get => QueryDictionary;
        set => QueryDictionary = value;
    }
}


[GrpcService(Name = "ProductService")]
public interface IProductGrpcService
{
    [GrpcToRestApi(RouteTemplate = "api/products")]
    Task<List<Product>> GetProducts(ProductFilterQuery filterQuery);

    [GrpcToRestApi(Method = "GET", RouteTemplate = "api/products/{id}")]
    Task<Product> GetProduct(long id);
}

public class ProductGrpcService : IProductGrpcService
{
    private static readonly List<Product> _products;

    static ProductGrpcService()
    {
        var faker = new Faker();

        _products = new List<Product>();

        for (var i = 0; i < 512; i++)
        {
            var product = new Product
            {
                Name        = faker.Commerce.ProductName(),
                Description = faker.Commerce.ProductDescription(),
                Price       = faker.Random.Decimal(1, 1000),
                Id          = i + 1
            };

            _products.Add(product);
        }
    }

    public async Task<Product> GetProduct(long id)
    {
        return _products.Find(p => p.Id == id);
    }

    public async Task<List<Product>> GetProducts(ProductFilterQuery filterQuery)
    {
        var entitiesQuery = _products.AsQueryable();

        entitiesQuery = entitiesQuery.ApplyDeepFilters(filterQuery.QueryDictionary);

        var orderedEntitiesQuery = entitiesQuery.OrderBy(nameof(Product.Id).ToMemberAccessExpression<Product>());

        if (filterQuery.OrderedColumns.Count > 0)
        {
            var sortInitialized = false;

            foreach (var orderByColumn in filterQuery.OrderedColumns)
            {
                var actualColumnName = orderByColumn.TrimStart('-');

                try
                {

                    var orderByColumnExpr = actualColumnName.ToMemberAccessExpression<Product>();

                    if (orderByColumn.StartsWith("-"))
                    {
                        orderedEntitiesQuery = !sortInitialized
                                                   ? entitiesQuery.OrderByDescending(orderByColumnExpr)
                                                   : orderedEntitiesQuery.ThenByDescending(orderByColumnExpr);
                    }
                    else
                    {
                        orderedEntitiesQuery = !sortInitialized
                                                   ? entitiesQuery.OrderBy(orderByColumnExpr)
                                                   : orderedEntitiesQuery.ThenBy(orderByColumnExpr);
                    }

                    if (!sortInitialized)
                        sortInitialized = true;
                }
                catch (UnknownPropertyException)
                {
                    continue;
                }
            }
        }

        var finalQuery = orderedEntitiesQuery.Skip(filterQuery.PageIndex * filterQuery.PageSize)
                                             .Take(filterQuery.PageSize);

        return finalQuery.ToList();
    }
}
