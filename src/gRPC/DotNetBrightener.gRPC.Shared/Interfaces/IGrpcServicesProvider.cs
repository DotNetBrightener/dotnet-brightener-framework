using System;
using System.Collections.Generic;

namespace DotNetBrightener.gRPC;

public interface IGrpcServicesProvider
{
    List<Type> ServiceTypes { get; }
}