using DotNetBrightener.gRPC;
using DotNetBrightener.WebApi.GenericCRUD.Models;

namespace CRUD_With_gRPC.Core;

public class Product
{

}

public class ProductFilterQuery: BaseQueryModel
{

}


[GrpcService]
public interface IProductService
{
    Task<Product> GetProduct(long id);

    Task<List<Product>> GetProducts(ProductFilterQuery filterQuery);
}

public class ProductService : IProductService
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
