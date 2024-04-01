using AspNet.Extensions.SelfDocumentedProblemResult;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;

// ReSharper disable once CheckNamespace
namespace Microsoft.AspNetCore.Mvc;

public static class ProblemResultExtensions
{
    internal static string               UiProblemUrl        = null;
    internal static IHttpContextAccessor HttpContextAccessor = null;


    /// <summary>
    ///     Creates an <see cref="ObjectResult"/> that produces a <see cref="ProblemDetails"/> response.
    /// </summary>
    /// <param name="problemResult">The <see cref="IProblemResult" /></param>
    /// <param name="reason">The reason of why the problem / error occured</param>
    /// <param name="instance">The value for <see cref="ProblemDetails.Instance" /></param>
    /// <returns></returns>
    public static ProblemDetailResult ToProblemResult(this IProblemResult problemResult,
                                                      string              reason   = null,
                                                      string              instance = null)
    {
        return new ProblemDetailResult(problemResult, reason, instance);
    }

    /// <summary>
    ///     Creates an <see cref="ObjectResult"/> that produces a <see cref="ProblemDetails"/> response.
    /// </summary>
    /// <param name="problemResult">The <see cref="IProblemResult" /></param>
    /// <returns></returns>
    public static ProblemDetailResult ToProblemResult(this Exception problemResult,
                                                      HttpStatusCode statusCode = HttpStatusCode.InternalServerError)
    {
        return new ProblemDetailResult(problemResult, statusCode);
    }

    /// <summary>
    ///     Creates a <see cref="ProblemDetails"/> response.
    /// </summary>
    /// <param name="problemResult">The <see cref="IProblemResult" /></param>
    /// <param name="reason">The reason of why the problem / error occured</param>
    /// <param name="instance">The value for <see cref="ProblemDetails.Instance" /></param>
    /// <returns></returns>
    public static ProblemDetails ToProblemDetails(this IProblemResult problemResult,
                                                  string              reason   = null,
                                                  string              instance = null)
    {
        var extensionDictionary = new Dictionary<string, object>();

        if (problemResult is Exception ex)
        {
            var message = ex.Message;

            if (!string.IsNullOrEmpty(message))
                extensionDictionary.TryAdd("message", message);

            var fullExceptionMessage = ex.GetFullExceptionMessage();

            if (!string.IsNullOrEmpty(fullExceptionMessage) &&
                message != fullExceptionMessage)
                extensionDictionary.TryAdd("fullErrorMessage", fullExceptionMessage);
        }

        if (problemResult.Data.Count > 0)
        {
            foreach (DictionaryEntry dictionaryEntry in problemResult.Data)
            {
                var key = dictionaryEntry.Key.ToString();
                if (!string.IsNullOrEmpty(key))
                    extensionDictionary.TryAdd(key, dictionaryEntry.Value);
            }
        }

        if (!string.IsNullOrEmpty(problemResult.ErrorCode))
            extensionDictionary.TryAdd("errorCode", problemResult.ErrorCode);


        var type = problemResult.ErrorCode;

        if (!string.IsNullOrEmpty(UiProblemUrl))
        {
            if (HttpContextAccessor?.HttpContext != null)
            {
                var currentRequestUrl = HttpContextAccessor.GetRequestUrl();
                type = new Uri(new Uri(currentRequestUrl),
                               UiProblemUrl + "#/" + problemResult.ErrorCode)
                   .ToString();
            }
            else
            {
                type = "##your_domain.com##" + UiProblemUrl + "#/" + problemResult.ErrorCode;
            }
        }

        return new ProblemDetails
        {
            Type = type,
            Title = problemResult.Title,
            Status = problemResult.StatusCode != 0 ? problemResult.StatusCode : (int)HttpStatusCode.InternalServerError,
            Detail = (reason ?? problemResult.DetailReason)?.Replace("`", ""),
            Instance = instance ?? HttpContextAccessor?.HttpContext?.Request.Path,
            Extensions = extensionDictionary
        };
    }

    /// <summary>
    ///     Creates a <see cref="ProblemDetails"/> object from the given <see cref="Exception"/>
    /// </summary>
    /// <param name="exception"></param>
    /// <param name="statusCode"></param>
    /// <returns></returns>
    public static ProblemDetails ToProblemDetails(this Exception  exception,
                                                  HttpStatusCode? statusCode = null)
    {
        var problemDetails = new ProblemDetails
        {
            Title    = exception.GetType().Name,
            Detail   = exception.GetFullExceptionMessage(),
            Status   = statusCode.HasValue ? (int)statusCode : (int)HttpStatusCode.InternalServerError,
            Type     = exception.GetType().FullName,
            Instance = HttpContextAccessor?.HttpContext?.Request.Path
        };

        if (exception.Data.Count > 0)
        {
            foreach (DictionaryEntry dictionaryEntry in exception.Data)
            {
                var key = dictionaryEntry.Key.ToString();
                if (!string.IsNullOrEmpty(key))
                    problemDetails.Extensions.TryAdd(key, dictionaryEntry.Value);
            }
        }

        return problemDetails;
    }
}