using System.Diagnostics;
using Microsoft.AspNetCore.Http;

namespace DotNetBrightener.GenericCRUD.Models;

public static class BaseQueryModelExtensions
{
    [DebuggerStepThrough]
    public static TOutputModel ToQueryModel<TOutputModel>(this Dictionary<string, string> queryDictionary)
        where TOutputModel : BaseQueryModel, new()
    {
        var baseQueryModel = BaseQueryModel.FromDictionary<TOutputModel>(queryDictionary);

        return baseQueryModel;
    }

    [DebuggerStepThrough]
    public static TOutputModel ToQueryModel<TOutputModel>(this IQueryCollection query)
        where TOutputModel : BaseQueryModel, new()
    {
        var baseQueryModel = BaseQueryModel.FromQuery<TOutputModel>(query);

        return baseQueryModel;
    }
}