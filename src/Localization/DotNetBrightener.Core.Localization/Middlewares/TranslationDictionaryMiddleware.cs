using System.Globalization;
using System.Net;
using DotNetBrightener.Core.Localization.Services;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace DotNetBrightener.Core.Localization.Middlewares;

public class TranslationDictionaryMiddleware
{
    private readonly RequestDelegate _next;
    private readonly string          _requestPath;

    public TranslationDictionaryMiddleware(string requestPath, RequestDelegate next)
    {
        _requestPath = requestPath;
        _next        = next;
    }

    public Task Invoke(HttpContext          context,
                       ILocalizationManager localizationManager)
    {
        var languages = context.Request
                               .Cookies
                               .Where(_ => _.Key == nameof(HttpRequestHeader.AcceptLanguage))
                               .Select(_ => _.Value)
                               .ToArray();
            
        if (languages.Length == 0)
        {
            languages = context.Request
                               .GetTypedHeaders()
                               .AcceptLanguage?
                               .OrderByDescending(x => x.Quality ?? 1)
                               .Select(x => x.Value.ToString())
                               .ToArray() ?? Array.Empty<string>();
        }

        if (languages.Length > 0)
        {
            var detectedLanguage = CultureInfo.CreateSpecificCulture(languages[0]);

            Thread.CurrentThread.CurrentUICulture = detectedLanguage;
            context.StoreCurrentCulture(detectedLanguage);
        }

        if (!context.Request.Path.StartsWithSegments(new PathString(_requestPath)))
            return _next.Invoke(context);

        var currentCulture = Thread.CurrentThread.CurrentUICulture;

        if (context.Request.Query.ContainsKey("language"))
        {
            var languageValue = context.Request.Query["language"];
            currentCulture = CultureInfo.CreateSpecificCulture(languageValue);
        }

        var dictionary = localizationManager.GetDictionary(currentCulture);

        context.Response.StatusCode  = (int) HttpStatusCode.OK;
        context.Response.ContentType = "application/json";

        return context.Response.WriteAsync(JsonConvert.SerializeObject(dictionary.Translations));
    }
}