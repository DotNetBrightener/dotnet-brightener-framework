using CRUD_With_gRPC.Core;

namespace CRUD_With_gRPC;

public class GrpcServiceProvider
{
    public List<Type> ServiceTypes =
    [
        typeof(IProductGrpcService)
    ];
}
