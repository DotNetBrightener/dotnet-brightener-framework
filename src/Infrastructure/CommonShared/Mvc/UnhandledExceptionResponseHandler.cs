using DotNetBrightener.WebApp.CommonShared.Exceptions;
using DotNetBrightener.WebApp.CommonShared.Models;
using DotNetBrightener.WebApp.CommonShared.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace DotNetBrightener.WebApp.CommonShared.Mvc;

public class UnhandledExceptionResponseHandler : IExceptionFilter, IActionFilterProvider
{
    private const string ApplicationJsonType = "application/json";

    private readonly ILogger                                 _logger;
    private readonly IErrorResultFactory                     _errorResultFactory;
    private readonly IEnumerable<IUnhandledExceptionHandler> _unhandledExceptionHandlers;
    private readonly IStringLocalizer                        T;

    public UnhandledExceptionResponseHandler(ILogger<UnhandledExceptionResponseHandler> logger,
                                             IEnumerable<IUnhandledExceptionHandler> unhandledExceptionHandlers,
                                             IStringLocalizer<UnhandledExceptionResponseHandler> localizer,
                                             IErrorResultFactory errorResultFactory)
    {
        _logger                     = logger;
        _errorResultFactory         = errorResultFactory;
        T                           = localizer;
        _unhandledExceptionHandlers = unhandledExceptionHandlers;
    }

    public void OnException(ExceptionContext context)
    {
        ContentResult defaultResult;

        var errorResult = _errorResultFactory.InstantiateErrorResult<DefaultErrorResult>();

        if (!string.IsNullOrEmpty(context.Exception.Message))
        {
            errorResult.ErrorMessage = T[context.Exception.Message];
        }

        errorResult.FullErrorMessage = context.Exception.GetFullExceptionMessage();
        errorResult.Data             = context.Exception.Data;
        int statusCode = (int)HttpStatusCode.InternalServerError;

        switch (context.Exception)
        {
            case ExceptionWithStatusCode exception:
            {
                var exceptionType = exception.GetType().FullName;

                if (exceptionType != typeof(ExceptionWithStatusCode).FullName)
                {
                    errorResult.ErrorType = exceptionType;
                }

                statusCode = (int)exception.StatusCode;

                break;
            }
            case UnauthorizedAccessException:
                statusCode = (int)HttpStatusCode.Unauthorized;

                break;
            case NotSupportedException:
                statusCode = (int)HttpStatusCode.BadRequest;

                break;
            case BadHttpRequestException httpRequestException:
                statusCode = httpRequestException.StatusCode;

                break;
            default:
                errorResult.ErrorType = context.Exception.GetType().FullName;

                break;
        }

        defaultResult = new ContentResult
        {
            Content     = JsonConvert.SerializeObject(errorResult, DefaultJsonSerializer.DefaultJsonSerializerSettings),
            ContentType = ApplicationJsonType,
            StatusCode  = statusCode
        };

        var exceptionContext = new UnhandledExceptionContext
        {
            ErrorResult      = errorResult,
            ContextException = context.Exception,
            ProcessResult    = defaultResult,
            StatusCode       = (HttpStatusCode)defaultResult.StatusCode
        };

        if (_unhandledExceptionHandlers.Any())
        {
            foreach (var handler in _unhandledExceptionHandlers)
            {
                handler.HandleException(exceptionContext);

                if (exceptionContext.ProcessResult != null)
                    break;
            }
        }

        _logger.LogError(context.Exception,
                         $"Unhandled Exception Occurred. Responded with HttpStatusCode {exceptionContext.StatusCode}");

        context.ExceptionHandled = true;
        context.Result           = exceptionContext.ProcessResult ?? defaultResult;
    }
}