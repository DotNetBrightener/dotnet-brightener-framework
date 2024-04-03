using System.Net;
using AspNet.Extensions.SelfDocumentedProblemResult;
using LanguageExts.Results;
using Microsoft.AspNetCore.Mvc;

// ReSharper disable once CheckNamespace
namespace Microsoft.AspNetCore.Http;

public static class ResultTypeEndpointExtensions
{

    /// <summary>
    ///     Converts a <see cref="Result{TValue, TError}" /> to an <see cref="IResult" />,
    ///     optionally convert the data before returning
    /// </summary>
    /// <param name="result">The <see cref="Result{TValue, TError}" /> object</param>
    /// <returns>
    ///     The <see cref="IResult" /> object
    /// </returns>
    public static IResult ToResult<TValue, TError>(this Result<TValue, TError> result,
                                                   Func<TValue, object>        converter = null,
                                                   HttpStatusCode              statusCode = HttpStatusCode.OK)
    {
        return result.Match(success => MatchSuccess(success, statusCode, converter),
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