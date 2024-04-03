using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;

namespace AspNet.Extensions.SelfDocumentedProblemResult.Filters;

/// <summary>
///     Marks a controller to handle the specified <see cref="ExceptionType"/> with given <see cref="StatusCode"/>
/// </summary>
public class ExceptionHandleWithStatusCodeAttribute : ExceptionFilterAttribute
{
    /// <summary>
    ///     The type of the exception to handle
    /// </summary>
    public Type ExceptionType { get; init; }

    /// <summary>
    ///     The status code to response
    /// </summary>
    public HttpStatusCode StatusCode { get; init; }

    /// <summary>
    ///     Instantiates an instance of <see cref="ExceptionHandleWithStatusCodeAttribute"/> class with given <see cref="ExceptionType"/>,
    ///     by default throw 500 error
    /// </summary>
    public ExceptionHandleWithStatusCodeAttribute(Type exceptionType)
        : this(exceptionType, HttpStatusCode.InternalServerError)
    {

    }

    /// <summary>
    ///     Instantiates an instance of <see cref="ExceptionHandleWithStatusCodeAttribute"/> class with given <see cref="ExceptionType"/> and <see cref="StatusCode"/>
    /// </summary>
    public ExceptionHandleWithStatusCodeAttribute(Type exceptionType, HttpStatusCode statusCode)
    {
        ExceptionType = exceptionType;
        StatusCode    = statusCode;
    }


    public override void OnException(ExceptionContext context)
    {
        if (context.ExceptionHandled)
            return;

        var exceptionType = context.Exception.GetType();

        if (exceptionType == ExceptionType ||
            ExceptionType.IsAssignableFrom(exceptionType) ||
            ExceptionType.IsAssignableTo(exceptionType))
        {
            var exception = context.Exception;

            ProblemDetails problemDetails;

            if (exception is IProblemResult exceptionWithCode)
            {
                problemDetails = exceptionWithCode.ToProblemDetails();
            }
            else
            {
                problemDetails = exception.ToProblemDetails(StatusCode);
            }


            problemDetails.Instance = context.HttpContext.Request.Path;

            context.Result           = new ProblemDetailResult(problemDetails);
            context.ExceptionHandled = true;
        }
    }
}