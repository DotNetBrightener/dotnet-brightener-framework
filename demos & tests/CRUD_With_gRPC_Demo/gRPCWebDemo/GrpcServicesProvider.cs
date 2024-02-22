using CRUD_With_gRPC.Core;
using DotNetBrightener.gRPC;

namespace gRPCWebDemo;

public class GrpcServicesProvider : IGrpcServicesProvider
{
    public List<Type> ServiceTypes { get; } =
    [
        typeof(IProductGrpcService)
    ];
}