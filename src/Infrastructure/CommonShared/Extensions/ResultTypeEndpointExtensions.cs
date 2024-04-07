using System.Net;
using AspNet.Extensions.SelfDocumentedProblemResult;
using LanguageExts.Results;
using Microsoft.AspNetCore.Mvc;

// ReSharper disable once CheckNamespace
namespace Microsoft.AspNetCore.Http;

public static class ResultTypeEndpointExtensions
{

    /// <summary>
    ///     Converts a <see cref="Result{TValue, TError}" /> to an <see cref="IResult" />
    ///     with status code <c>200 (OK)</c>.<br /> 
    ///     Depends on the state of the result, it can be a success result, or an error result
    /// </summary>
    /// <param name="result">
    ///     The <see cref="Result{TValue, TError}" /> object
    /// </param>
    /// <returns>
    ///     The <see cref="IResult" /> object
    /// </returns>
    public static IResult ToResult<TValue, TError>(this Result<TValue, TError> result)
        => ToResult(result, null, HttpStatusCode.OK);

    /// <summary>
    ///     Converts a <see cref="Result{TValue, TError}" /> to an <see cref="IResult" />
    ///     with specified <param name="successStatusCode" />.<br /> 
    ///     Depends on the state of the result, it can be a success result, or an error result
    /// </summary>
    /// <param name="result">
    ///     The <see cref="Result{TValue, TError}" /> object
    /// </param>
    /// <param name="successStatusCode">
    ///     The <see cref="HttpStatusCode"/> value if the <param name="result" /> returns success value
    /// </param>
    /// <returns>
    ///     The <see cref="IResult" /> object
    /// </returns>
    public static IResult ToResult<TValue, TError>(this Result<TValue, TError> result,
                                                   HttpStatusCode              successStatusCode)
        => ToResult(result, null, successStatusCode);

    /// <summary>
    ///     Converts a <see cref="Result{TValue, TError}" /> to an <see cref="IResult" />,
    ///     using the specified <param name="converter" />  to convert the data before responding
    ///     with status code <c>200 (OK)</c> . <br />
    /// 
    ///     Depends on the state of the result, it can be a success result, or an error result
    /// </summary>
    /// <param name="result">
    ///     The <see cref="Result{TValue, TError}" /> object
    /// </param>
    /// <param name="converter">
    ///     The function to convert map or format the success value to a different object
    /// </param>
    /// <returns>
    ///     The <see cref="IResult" /> object
    /// </returns>
    public static IResult ToResult<TValue, TError>(this Result<TValue, TError> result,
                                                   Func<TValue, object>        converter) =>
        ToResult(result, converter, HttpStatusCode.OK);

    /// <summary>
    ///     Converts a <see cref="Result{TValue, TError}" /> to an <see cref="IResult" />,
    ///     using the specified <param name="converter" />  to convert the data before responding
    ///     with specified <param name="successStatusCode" />.<br /> 
    ///     Depends on the state of the result, it can be a success result, or an error result
    /// </summary>
    /// <param name="result">
    ///     The <see cref="Result{TValue, TError}" /> object
    /// </param>
    /// <param name="converter">
    ///     The function to convert map or format the success value to a different object
    /// </param>
    /// <param name="successStatusCode">
    ///     The <see cref="HttpStatusCode"/> value if the <param name="result" /> returns success value
    /// </param>
    /// <returns>
    ///     The <see cref="IResult" /> object
    /// </returns>
    public static IResult ToResult<TValue, TError>(this Result<TValue, TError> result,
                                                   Func<TValue, object>        converter,
                                                   HttpStatusCode              successStatusCode)
    {
        return result.Match(success => MatchSuccess(success, successStatusCode, converter),
                            MatchErrorToResult);
    }

    private static IResult MatchSuccess<TValue, TOut>(TValue             success,
                                                      HttpStatusCode     statusCode,
                                                      Func<TValue, TOut> converter = null)
    {
        if (converter is null)
            return Results.Json(success, statusCode: (int)statusCode);

        var value = converter(success);

        return Results.Json(value, statusCode: (int)statusCode);
    }

    private static IResult MatchErrorToResult<TError>(TError error)
    {
        var problemDetails = error switch
        {
            IProblemResult problemResult => problemResult.ToProblemDetails(),
            Exception exception          => exception.ToProblemDetails(),
            _                            => null
        };

        return problemDetails is null
                   ? Results.Json(error, statusCode: 500)
                   : Results.Json(problemDetails, statusCode: problemDetails.Status);
    }
}