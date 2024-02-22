using System;
using Grpc.Core;

namespace DotNetBrightener.gRPC.Extensions;

public static class ServerCallContextExtensions
{
    public static bool IsRestRequest(this ServerCallContext context)
    {
        var currentContext = context.GetHttpContext();

        return !currentContext.Request.Protocol.Equals("HTTP/2", StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsGrpcRequest(this ServerCallContext context)
    {
        var currentContext = context.GetHttpContext();

        return currentContext.Request.Protocol.Equals("HTTP/2", StringComparison.OrdinalIgnoreCase);
    }
}