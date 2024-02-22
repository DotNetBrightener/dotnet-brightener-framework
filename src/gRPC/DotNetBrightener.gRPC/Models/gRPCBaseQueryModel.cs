using System.Collections.Generic;
using DotNetBrightener.GenericCRUD.Models;

namespace DotNetBrightener.gRPC.Models;

// ReSharper disable once InconsistentNaming

public class gRPCBaseQueryModel : BaseQueryModel
{
    public Dictionary<string, string> Filters
    {
        get => QueryDictionary;
        set => QueryDictionary = value;
    }
}