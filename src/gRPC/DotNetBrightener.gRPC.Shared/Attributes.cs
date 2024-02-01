using System;

namespace DotNetBrightener.gRPC;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public class GrpcMessageAttribute : Attribute
{
    public string Name { get; set; }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
public class GrpcServiceAttribute : Attribute
{
    public string? Name { get; set; }
}

[AttributeUsage(AttributeTargets.Method)]
public class GrpcToRestApiAttribute : Attribute
{
    private string _routeTemplate;
    public  string Method { get; set; } = "GET";

    public string RouteTemplate
    {
        get => _routeTemplate;
        set
        {
            if (value.StartsWith("/"))
            {
                _routeTemplate = value;
            }
            else

            {
                _routeTemplate = "/" + value;
            }
        }
    }
}