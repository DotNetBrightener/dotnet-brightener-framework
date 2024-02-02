using CRUD_With_gRPC.Core;
using DotNetBrightener.gRPC;

namespace gRPCWebDemo;

public class GrpcServiceProvider : IGrpcServiceProvider
{
    public List<Type> ServiceTypes { get; } =
    [
        typeof(IProductGrpcService)
    ];
}