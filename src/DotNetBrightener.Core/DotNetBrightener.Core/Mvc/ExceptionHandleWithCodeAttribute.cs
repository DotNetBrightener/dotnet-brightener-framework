using System;
using System.Net;
using DotNetBrightener.Core.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;

namespace DotNetBrightener.Core.Mvc
{
    /// <summary>
    ///     Marks a controller to handle exception with given status code
    /// </summary>
    public class ExceptionHandleWithCodeAttribute : ExceptionFilterAttribute
    {
        private const string ApplicationJsonType = "application/json";

        public Type ExceptionType { get; set; }

        public HttpStatusCode StatusCode { get; set; }

        /// <summary>
        ///     Instantiates an instance of <see cref="ExceptionHandleWithCodeAttribute"/> class with default settings
        /// </summary>
        public ExceptionHandleWithCodeAttribute()
        {

        }

        /// <summary>
        ///     Instantiates an instance of <see cref="ExceptionHandleWithCodeAttribute"/> class with given <see cref="ExceptionType"/>, by default to thrown 500 error
        /// </summary>
        public ExceptionHandleWithCodeAttribute(Type exceptionType) : this(exceptionType, HttpStatusCode.InternalServerError)
        {

        }

        /// <summary>
        ///     Instantiates an instance of <see cref="ExceptionHandleWithCodeAttribute"/> class with given <see cref="ExceptionType"/> and <see cref="StatusCode"/>
        /// </summary>
        public ExceptionHandleWithCodeAttribute(Type exceptionType, HttpStatusCode statusCode)
        {
            ExceptionType = exceptionType;
            StatusCode = statusCode;
        }


        public override void OnException(ExceptionContext context)
        {
            if (context.ExceptionHandled)
                return;

            var errorResultFactory = context.HttpContext.RequestServices.GetService<IErrorResultFactory>();
            var T = context.HttpContext.RequestServices
                           .GetService<IStringLocalizer<ExceptionHandleWithCodeAttribute>>();

            if (context.Exception.GetType() == ExceptionType)
            {
                var exception = context.Exception;
                var exceptionType = exception.GetType().FullName;

                var errorResult = errorResultFactory.InstantiateErrorResult<DefaultErrorResult>();

                errorResult.ErrorMessage = T[exception.Message];
                errorResult.FullErrorMessage = exception.GetFullExceptionMessage();

#if DEBUG
                errorResult.StackTrace = context.Exception.StackTrace;
#endif

                var statusCode = StatusCode;
                if (exception is ExceptionWithStatusCode exceptionWithCode)
                {
                    statusCode = exceptionWithCode.StatusCode;
                }

                if (exceptionType != typeof(ExceptionWithStatusCode).FullName)
                {
                    errorResult.ErrorType = exceptionType;
                }

                context.Result = new ContentResult
                {
                    Content = JsonConvert.SerializeObject(errorResult),
                    ContentType = ApplicationJsonType,
                    StatusCode = (int)statusCode
                };

                context.ExceptionHandled = true;
            }
        }
    }
}