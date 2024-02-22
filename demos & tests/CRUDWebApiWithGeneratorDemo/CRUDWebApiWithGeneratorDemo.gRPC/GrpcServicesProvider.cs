using CRUDWebApiWithGeneratorDemo.gRPC.GrpcServices;
using DotNetBrightener.gRPC;

namespace CRUDWebApiWithGeneratorDemo.gRPC;

public class GrpcServicesProvider : IGrpcServicesProvider
{
    public List<Type> ServiceTypes { get; } =
    [
        //typeof(IProductGrpcService)
    ];
}