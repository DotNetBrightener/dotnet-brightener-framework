using DotNetBrightener.gRPC;
using DotNetBrightener.WebApi.GenericCRUD.Models;

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
    
}


[GrpcService(Name = "ProductService")]
public interface IProductGrpcService
{
    //[GrpcToRestApi(RouteTemplate = "api/products")]
    //Task<List<Product>> GetProducts(ProductFilterQuery filterQuery);
    
    [GrpcToRestApi(Method = "GET", RouteTemplate = "api/products/{id}")]
    Task<Product> GetProduct(long id);
}

public class ProductGrpcService : IProductGrpcService
{
    public async Task<Product> GetProduct(long id)
    {
        return null;
    }

    public Task<List<Product>> GetProducts(ProductFilterQuery filterQuery)
    {
        throw new NotImplementedException();
    }
}
