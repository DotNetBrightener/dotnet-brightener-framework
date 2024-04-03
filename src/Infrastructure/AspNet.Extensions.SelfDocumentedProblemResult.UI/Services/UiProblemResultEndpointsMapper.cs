using AspNet.Extensions.SelfDocumentedProblemResult.ExceptionHandlers;
using DotNetBrightener.Extensions.ProblemsResult.UI;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;

namespace AspNet.Extensions.SelfDocumentedProblemResult.UI.Services;

internal class UiProblemResultEndpointsMapper
{
    private readonly List<Type> _problemTypes;

    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        WriteIndented        = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public UiProblemResultEndpointsMapper(List<Type> problemTypes)
    {
        _problemTypes = problemTypes;
    }

    public IEnumerable<IEndpointConventionBuilder> Map(IEndpointRouteBuilder   builder,
                                                       ErrorsDocTrackerOptions errorsDocTrackerOptions)
    {
        var endpoints   = new List<IEndpointConventionBuilder>();
        var siteBarNavs = new List<SiteBarNavItems>();

        var environment = builder.ServiceProvider.GetService(typeof(IWebHostEnvironment)) as IWebHostEnvironment;

        if (environment == null)
        {
            throw new InvalidOperationException("IWebHostEnvironment is not registered in the service container");
        }

        var mardownPath = Path.Combine(environment.ContentRootPath, ".md_files");

        if (Directory.Exists(mardownPath))
        {
            Directory.Delete(mardownPath, true);
        }

        Directory.CreateDirectory(mardownPath);
        

        foreach (var type in _problemTypes)
        {
            var problemResult = Activator.CreateInstance(type) as IProblemResult;

            if (problemResult == null)
            {
                continue;
            }

            siteBarNavs.Add(new SiteBarNavItems
            {
                Name      = problemResult.Title,
                ErrorCode = problemResult.ErrorCode,
                Url       = $"{problemResult.ErrorCode}.md"
            });

            var content = BuildErrorDocContent(problemResult);

            var filePath = Path.Combine(mardownPath, $"{problemResult.ErrorCode}.md");
            File.WriteAllText(filePath, content);
        }

        var sidebarContent = BuildSiteBarContent(siteBarNavs);

        var sideBarFilePath = Path.Combine(mardownPath, "_sidebar.md");
        File.WriteAllText(sideBarFilePath, sidebarContent);

        endpoints.Add(builder.MapGet($"{errorsDocTrackerOptions.UiPath}/{{fileName}}.md",
                                     async (string fileName, IHttpContextAccessor accessor) =>
                                     {
                                         if (string.IsNullOrEmpty(fileName))
                                             return Results.NotFound();

                                         var filePath = Path.Combine(mardownPath, $"{fileName}.md");

                                         if (!File.Exists(filePath))
                                         {
                                             return Results.NotFound();
                                         }

                                         var content = await File.ReadAllTextAsync(filePath)
                                                                 .ConfigureAwait(false);

                                         if (accessor.HttpContext is not null)
                                         {
                                             var domainReplacement = new Uri(new Uri(accessor.GetRequestUrl()), "/")
                                                                    .ToString()
                                                                    .TrimEnd('/');

                                             content = content.Replace("##your_domain.com##",
                                                                       domainReplacement);
                                         }

                                         return Results.Content(content, ContentType.MARKDOWN);
                                     }));

        return endpoints;
    }

    private string BuildSiteBarContent(List<SiteBarNavItems> siteBarNavs)
    {
        var stringBuilder = new StringBuilder();

        stringBuilder.AppendLine($"- [Home](/)");
        stringBuilder.AppendLine();
        stringBuilder.AppendLine($"- Errors List");
        stringBuilder.AppendLine();

        foreach (var siteBarNav in siteBarNavs.OrderBy(_ => _.ErrorCode))
        {
            stringBuilder.AppendLine($"  - [{siteBarNav.ErrorCode}: {siteBarNav.Name}]({siteBarNav.Url})");
        }

        var content = stringBuilder.ToString();

        return content;
    }

    private string BuildErrorDocContent(IProblemResult problemResult)
    {
        var stringBuilder = new StringBuilder();

        stringBuilder.AppendLine($"#  {problemResult.Title} `{problemResult.ErrorCode}`");
        stringBuilder.AppendLine($"> HTTP Status Code `{problemResult.StatusCode}`");
        stringBuilder.AppendLine();
        stringBuilder.AppendLine("### Summary");
        stringBuilder.AppendLine();
        stringBuilder.AppendLine(problemResult.Summary);

        var problemResultReason = problemResult.DetailReason;

        if (!string.IsNullOrEmpty(problemResultReason))
        {
            stringBuilder.AppendLine();
            stringBuilder.AppendLine("### Detail reason(s) why the error might occur");
            stringBuilder.AppendLine();
            stringBuilder.AppendLine(problemResultReason);
        }

        if (problemResult is Exception)
        {
            stringBuilder.AppendLine();
            stringBuilder.AppendLine("### Throwable");
            stringBuilder.AppendLine();
            stringBuilder
               .AppendLine($"This error is derived from `System.Exception`, therefore, it might be thrown. If the exception is thrown and remains unhandled by the application's code, it will result in the failure of the request, accompanied by the status code outlined in [Response HTTP Status Code](#response-http-status-code).");
        }

        if (problemResult.StatusCode != 0)
        {
            stringBuilder.AppendLine();
            stringBuilder.AppendLine("### Response HTTP Status Code");
            stringBuilder.AppendLine();
            stringBuilder
               .AppendLine($"The request that encounters this error shall be responded with HTTP Status Code `{problemResult.StatusCode}`.");

            if (UnhandledExceptionResponseHandler.StatusCodeToTypeLink.TryGetValue(problemResult.StatusCode,
                                                                                   out var rfcLink))
                stringBuilder
                   .AppendLine($"> Additionally, refer to [RFC 9110 Document Specification for Status Code `{problemResult.StatusCode}`]({rfcLink}).");
        }


        stringBuilder.AppendLine("### Response Body");
        stringBuilder.AppendLine();
        stringBuilder
           .AppendLine("Below is the sample response body of the request that encounters this error. Depends on the error, there can be additional properties that help you identify the issue.");
        stringBuilder.AppendLine($"```json");
        stringBuilder.AppendLine(JsonSerializer.Serialize(problemResult
                                                             .ToProblemDetails(instance:
                                                                               "{usually is URL of the request}"),
                                                          JsonSerializerOptions));
        stringBuilder.AppendLine($"```");

        return stringBuilder.ToString();
    }
}

internal class SiteBarNavItems
{
    public string Name      { get; set; }
    public string Url       { get; set; }
    public string ErrorCode { get; set; }
}