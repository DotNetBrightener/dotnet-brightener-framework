using DotNetBrightener.Core.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;

namespace DotNetBrightener.Core.Mvc
{
    public class UnhandledExceptionResponseHandler : IExceptionFilter, IActionFilterProvider
    {
        private const string ApplicationJsonType = "application/json";

        private readonly ILogger _logger;
        private readonly IErrorResultFactory _errorResultFactory;
        private readonly IEnumerable<IUnhandleExceptionHandler> _unhandledExceptionHandlers;
        private readonly IStringLocalizer T;

        public UnhandledExceptionResponseHandler(ILogger<UnhandledExceptionResponseHandler> logger,
                                                 IErrorResultFactory errorResultFactory,
                                                 IEnumerable<IUnhandleExceptionHandler> unhandledExceptionHandlers,
                                                 IStringLocalizer<UnhandledExceptionResponseHandler> localizer)
        {
            _logger = logger;
            _errorResultFactory = errorResultFactory;
            T = localizer;
            _unhandledExceptionHandlers = unhandledExceptionHandlers;
        }

        public void OnException(ExceptionContext context)
        {
            ContentResult defaultResult = null;
            var errorResult = _errorResultFactory.InstantiateErrorResult<DefaultErrorResult>();

#if DEBUG
            errorResult.StackTrace = context.Exception.StackTrace;
#endif
            if (!string.IsNullOrEmpty(context.Exception.Message))
            {
                errorResult.ErrorMessage = T[context.Exception.Message];
            }

            if (context.Exception is ExceptionWithStatusCode exception)
            {
                var exceptionType = exception.GetType().FullName;

                if (exceptionType != typeof(ExceptionWithStatusCode).FullName)
                {
                    errorResult.ErrorType = exceptionType;
                }

                defaultResult = new ContentResult
                {
                    Content = JsonConvert.SerializeObject(errorResult, CoreConstants.DefaultJsonSerializerSettings),
                    ContentType = ApplicationJsonType,
                    StatusCode = (int)exception.StatusCode
                };
            }

            else if (context.Exception is UnauthorizedAccessException)
            {
                defaultResult = new ContentResult
                {
                    Content = JsonConvert.SerializeObject(errorResult, CoreConstants.DefaultJsonSerializerSettings),
                    ContentType = ApplicationJsonType,
                    StatusCode = (int)HttpStatusCode.Unauthorized
                };
            }

            else if (context.Exception is BadHttpRequestException httpRequestException)
            {
                defaultResult = new ContentResult
                {
                    Content = JsonConvert.SerializeObject(errorResult, CoreConstants.DefaultJsonSerializerSettings),
                    ContentType = ApplicationJsonType,
                    StatusCode = httpRequestException.StatusCode
                };
            }

            else
            {
                _logger.LogError(context.Exception, "Unhandled Exception Occurred");

                errorResult.ErrorType = context.Exception.GetType().FullName;
                errorResult.FullErrorMessage = context.Exception.GetFullExceptionMessage();

                defaultResult = new ContentResult
                {
                    Content = JsonConvert.SerializeObject(errorResult, CoreConstants.DefaultJsonSerializerSettings),
                    ContentType = ApplicationJsonType,
                    StatusCode = (int)HttpStatusCode.InternalServerError
                };
            }

            if (context.HttpContext
                       .Request
                       .GetTypedHeaders()
                       .Accept.Contains(MediaTypeHeaderValue.Parse(ApplicationJsonType)))
            {

                context.ExceptionHandled = true;
                context.Result = defaultResult;
                return;
            }

            var exceptionContext = new UnhandledExceptionContext
            {
                ErrorResult = errorResult,
                ContextException = context.Exception,
                ProcessResult = defaultResult,
                StatusCode = (HttpStatusCode)defaultResult.StatusCode
            };

            foreach (var handler in _unhandledExceptionHandlers)
            {
                handler.HandleException(exceptionContext);
                if (exceptionContext.ProcessResult != null)
                {
                    break;
                }
            }

            context.ExceptionHandled = true;
            context.Result = exceptionContext.ProcessResult ?? defaultResult;
        }
    }
}