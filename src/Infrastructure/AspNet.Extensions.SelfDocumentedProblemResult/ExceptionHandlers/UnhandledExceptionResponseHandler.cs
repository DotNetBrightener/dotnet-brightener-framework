using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;

namespace AspNet.Extensions.SelfDocumentedProblemResult.ExceptionHandlers;

public class UnhandledExceptionResponseHandler : IExceptionHandler
{
    internal static readonly Dictionary<int, string> StatusCodeToTypeLink = new()
    {
        {
            400, "https://datatracker.ietf.org/doc/html/rfc9110/#name-400-bad-request"
        },
        {
            401, "https://datatracker.ietf.org/doc/html/rfc9110/#name-401-unauthorized"
        },
        {
            403, "https://datatracker.ietf.org/doc/html/rfc9110/#name-403-forbidden"
        },
        {
            404, "https://datatracker.ietf.org/doc/html/rfc9110/#name-404-not-found"
        },
        {
            405, "https://datatracker.ietf.org/doc/html/rfc9110/#name-405-method-not-allowed"
        },
        {
            406, "https://datatracker.ietf.org/doc/html/rfc9110/#name-406-not-acceptable"
        },
        {
            409, "https://datatracker.ietf.org/doc/html/rfc9110/#name-409-conflict"
        },
        {
            415, "https://datatracker.ietf.org/doc/html/rfc9110/#name-415-unsupported-media-type"
        },
        {
            500, "https://datatracker.ietf.org/doc/html/rfc9110/#name-500-internal-server-error"
        }
    };

    private readonly ILogger                _logger;
    private readonly IProblemDetailsService _problemDetailsService;
    private readonly IHttpContextAccessor   _httpContextAccessor;

    public UnhandledExceptionResponseHandler(ILogger<UnhandledExceptionResponseHandler> logger,
                                             IProblemDetailsService problemDetailsService ,
                                             IHttpContextAccessor                       httpContextAccessor)
    {
        _logger                     = logger;
        _problemDetailsService = problemDetailsService;
        _httpContextAccessor        = httpContextAccessor;

        ProblemResultExtensions.HttpContextAccessor ??= _httpContextAccessor;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext       httpContext,
                                                Exception         exception,
                                                CancellationToken cancellationToken)
    {
        var problemDetails = new ProblemDetails();

        if (!string.IsNullOrEmpty(exception.Message))
        {
            problemDetails.Detail = exception.Message;
        }

        problemDetails.Extensions = new Dictionary<string, object>
        {
            {
                "fullErrorMessage", exception.GetFullExceptionMessage()
            },
            {
                "data", exception.Data
            }
        };

        int statusCode = (int)HttpStatusCode.InternalServerError;

        switch (exception)
        {
            case IProblemResult ex:
                problemDetails = ex.ToProblemDetails();

                break;

            case UnauthorizedAccessException:
                statusCode           = (int)HttpStatusCode.Unauthorized;
                problemDetails.Title = "Unauthorized";

                break;

            case InvalidOperationException:
                statusCode           = (int)HttpStatusCode.BadRequest;
                problemDetails.Title = "Bad Request";

                break;

            case NotSupportedException:
                statusCode           = (int)HttpStatusCode.BadRequest;
                problemDetails.Title = "Bad Request";

                break;

            case BadHttpRequestException httpRequestException:
                statusCode = httpRequestException.StatusCode;

                problemDetails = httpRequestException.ToProblemDetails();

                break;

            default:
                problemDetails = exception.ToProblemDetails();

                break;
        }

        problemDetails.Status = statusCode;

        if (string.IsNullOrEmpty(problemDetails.Type) &&
            StatusCodeToTypeLink.TryGetValue(statusCode, out var typeLink))
        {
            problemDetails.Type = typeLink;
        }

        problemDetails.Instance = httpContext.Request.Path;

        _logger.LogError(exception,
                         "Unhandled exception of type {exceptionType} occurred. Responded with HttpStatusCode {statusCode}",
                         exception.GetType().FullName,
                         problemDetails.Status);

        httpContext.Response.StatusCode = problemDetails.Status!.Value;
        problemDetails.Extensions.TryAdd("requestId", httpContext.TraceIdentifier);

        return await _problemDetailsService
            .TryWriteAsync(new ProblemDetailsContext
            {
                HttpContext = httpContext,
                Exception = exception,
                ProblemDetails = problemDetails
            });
    }
}