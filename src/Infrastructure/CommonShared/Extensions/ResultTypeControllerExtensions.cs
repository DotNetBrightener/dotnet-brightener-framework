using System.Net;
using AspNet.Extensions.SelfDocumentedProblemResult;
using LanguageExts.Results;

// ReSharper disable once CheckNamespace
namespace Microsoft.AspNetCore.Mvc;

public static class ResultTypeControllerExtensions
{
    /// <summary>
    ///     Converts a <see cref="Result{TValue, TError}" /> to an <see cref="IActionResult" />.
    ///     Depends on the state of the result, it can be a success result, or an error result
    /// </summary>
    /// <param name="result">
    ///     The <see cref="Result{TValue, TError}" /> object
    /// </param>
    /// <returns>
    ///     The <see cref="IActionResult" /> object
    /// </returns>
    public static IActionResult ToActionResult<TValue, TError>(this Result<TValue, TError> result)
        => ToActionResult(result, null, HttpStatusCode.OK);

    /// <summary>
    ///     Converts a <see cref="Result{TValue, TError}" /> to an <see cref="IActionResult" />.
    ///     Depends on the state of the result, it can be a success result, or an error result
    /// </summary>
    /// <param name="result">
    ///     The <see cref="Result{TValue, TError}" /> object
    /// </param>
    /// <param name="successStatusCode">
    ///     The <see cref="HttpStatusCode"/> value if the <param name="result" /> returns success value
    /// </param>
    /// <returns>
    ///     The <see cref="IActionResult" /> object
    /// </returns>
    public static IActionResult ToActionResult<TValue, TError>(this Result<TValue, TError> result,
                                                               HttpStatusCode              successStatusCode)
        => ToActionResult(result, null, successStatusCode);

    /// <summary>
    ///     Converts a <see cref="Result{TValue, TError}" /> to an <see cref="IActionResult" />,
    ///     optionally convert the data before returning.
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
    ///     The <see cref="IActionResult" /> object
    /// </returns>
    public static IActionResult ToActionResult<TValue, TError>(this Result<TValue, TError> result,
                                                               Func<TValue, object>        converter)
        => ToActionResult(result, converter, HttpStatusCode.OK);

    /// <summary>
    ///     Converts a <see cref="Result{TValue, TError}" /> to an <see cref="IActionResult" />,
    ///     optionally convert the data before returning.
    /// 
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
    ///     The <see cref="IActionResult" /> object
    /// </returns>
    public static IActionResult ToActionResult<TValue, TError>(this Result<TValue, TError> result,
                                                               Func<TValue, object>        converter,
                                                               HttpStatusCode              successStatusCode)
    {
        return result.Match(success =>
                            {
                                if (converter is null)
                                    return new ObjectResult(success)
                                    {
                                        StatusCode = (int)successStatusCode
                                    };

                                var value = converter(success);

                                return new ObjectResult(value)
                                {
                                    StatusCode = (int)successStatusCode
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