namespace DotNetBrightener.gRPC.Generator.Tests;

public class TestHelpers
{
    internal const string AttributesSource = @"
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
    public  string Method { get; set; } = ""GET"";

    public string RouteTemplate
    {
        get => _routeTemplate;
        set
        {
            if (value.StartsWith(""/""))
            {
                _routeTemplate = value;
            }
            else

            {
                _routeTemplate = ""/"" + value;
            }
        }
    }
}";

    internal const string PagedCollectionSource = @"
using System.Collections.Generic;

namespace DotNetBrightener.GenericCRUD.Models;

public class PagedCollection<T>
{
    public IEnumerable<T> Items { get; set; }

    public int TotalCount { get; set; }

    public int PageIndex { get; set; }

    public int PageSize { get; set; }

    public int ResultCount { get; set; }
}

public class PagedCollection : PagedCollection<dynamic> { }";
}