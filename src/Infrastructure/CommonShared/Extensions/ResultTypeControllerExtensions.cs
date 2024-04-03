using System.Net;
using AspNet.Extensions.SelfDocumentedProblemResult;
using LanguageExts.Results;

// ReSharper disable once CheckNamespace
namespace Microsoft.AspNetCore.Mvc;

public static class ResultTypeControllerExtensions
{
    /// <summary>
    ///     Converts a <see cref="Result{TValue, TError}" /> to an <see cref="IActionResult" />,
    ///     optionally convert the data before returning
    /// </summary>
    /// <param name="result">The <see cref="Result{TValue, TError}" /> object</param>
    /// <returns>
    ///     The <see cref="IActionResult" /> object
    /// </returns>
    public static IActionResult ToActionResult<TValue, TError>(this Result<TValue, TError> result,
                                                               Func<TValue, object> converter = null,
                                                               HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        return result.Match(success =>
                            {
                                if (converter is null)
                                    return new ObjectResult(success)
                                    {
                                        StatusCode = (int)statusCode
                                    };

                                var value = converter(success);

                                return new ObjectResult(value)
                                {
                                    StatusCode = (int)statusCode
                                };
                            },
                            MatchErrorToResult);
    }

    private static IActionResult MatchErrorToResult<TError>(TError error)
    {
        if (error is IProblemResult problemResult)
        {
            return problemResult.ToProblemResult();
        }

        if (error is Exception exception)
        {
            return exception.ToProblemResult();
        }

        return new ObjectResult(error)
        {
            StatusCode = 500
        };
    }
}