
using Grpc.Core;
using CRUD_With_gRPC.Core;

namespace CRUD_With_gRPC.Services;

public partial class ProductServiceImpl : CRUD_With_gRPC.Core.ProductService.ProductServiceBase
{
    private readonly ILogger _logger;
    private readonly IProductGrpcService _productGrpcService;

    public ProductServiceImpl(
        ILogger<ProductServiceImpl> logger,
        IProductGrpcService productGrpcService)
    {
        _logger = logger;
        _productGrpcService = productGrpcService;
    }

    
    public override async Task<ProductResponse> GetProduct(
            GetProductRequest request, 
            ServerCallContext context)
    {
        // TODO: Implement GetProduct

        // var result = await _productGrpcService.GetProduct(request);

        return new ProductResponse
        {
            Price = 1.99f,
            Name = "Product 1",
            Description = "Product 1 Description",
            Id = 1
        };
    }


}