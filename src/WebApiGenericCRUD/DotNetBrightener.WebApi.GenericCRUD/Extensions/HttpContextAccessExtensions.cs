using DotNetBrightener.WebApi.GenericCRUD.ActionFilters;
using Newtonsoft.Json.Linq;

// ReSharper disable once CheckNamespace

namespace Microsoft.AspNetCore.Http;

public static class HttpContextAccessExtensions
{
    public static JObject ObtainRequestBodyAsJObject(this IHttpContextAccessor httpContextAccessor)
    {
        return RequestBodyReader.ObtainBodyAsJObject(httpContextAccessor);
    }

    public static TModel ObtainRequestBodyAs<TModel>(this IHttpContextAccessor httpContextAccessor)
    {
        return RequestBodyReader.ObtainBodyAs<TModel>(httpContextAccessor);
    }

    public static string ObtainRequestBody(this IHttpContextAccessor httpContextAccessor)
    {
        return RequestBodyReader.ObtainBody(httpContextAccessor);
    }
}