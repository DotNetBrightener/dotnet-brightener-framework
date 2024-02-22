using CRUDWebApiWithGeneratorDemo.Core.Entities;
using CRUDWebApiWithGeneratorDemo.gRPC.Models;
using CRUDWebApiWithGeneratorDemo.Services.Data;
using DotNetBrightener;
using DotNetBrightener.GenericCRUD.Models;
using DotNetBrightener.gRPC;
using DotNetBrightener.gRPC.Services;

namespace CRUDWebApiWithGeneratorDemo.gRPC.GrpcServices;

[GrpcService(Name = "ProductService")]
public interface IProductService: IBaseCRUDService<Product>, IDependency
{
    [GrpcToRestApi(RouteTemplate = "api/products")]
    Task<PagedCollection> GetProducts(ProductQueryModel filterQuery);

    [GrpcToRestApi(Method = "GET", RouteTemplate = "api/products/{id}")]
    Task<Product> GetProduct(long id);
}

public class ProductService : BaseCrudService<Product>, IProductService
{
    private readonly IProductDataService _dataService;

    public ProductService(IProductDataService dataService)
        : base(dataService)
    {
        _dataService = dataService;
    }

    public Task<Product> GetProduct(long id) => GetItem(id);

    public Task<PagedCollection> GetProducts(ProductQueryModel filterQuery) => GetList(filterQuery);
}
