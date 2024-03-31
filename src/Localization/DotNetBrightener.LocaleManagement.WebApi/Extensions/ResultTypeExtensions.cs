using System;
using AspNet.Extensions.SelfDocumentedProblemResult;
using LanguageExts.Results;

namespace Microsoft.AspNetCore.Mvc;

internal static class ResultTypeExtensions
{
    /// <summary>
    ///     Converts a <see cref="Result{TValue}" /> to an <see cref="IActionResult" />
    /// </summary>
    /// <param name="result">The <see cref="Result{TValue}" /> object</param>
    /// <returns>
    ///     The <see cref="IActionResult" /> object
    /// </returns>
    public static IActionResult ToActionResult<TValue>(this Result<TValue> result)
    {
        return result.Match<IActionResult>(success => new OkObjectResult(success),
                                           error =>
                                           {
                                               if (error is IProblemResult problemResult)
                                               {
                                                   return problemResult.ToProblemResult();
                                               }

                                               return error.ToProblemResult();
                                           });
    }

    /// <summary>
    ///     Converts a <see cref="Result{TValue, TError}" /> to an <see cref="IActionResult" />
    /// </summary>
    /// <param name="result">The <see cref="Result{TValue, TError}" /> object</param>
    /// <returns>
    ///     The <see cref="IActionResult" /> object
    /// </returns>
    public static IActionResult ToActionResult<TValue, TError>(this Result<TValue, TError> result)
    {
        return result.Match<IActionResult>(
                                           success => new OkObjectResult(success),
                                           error =>
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
                                           });
    }

    /// <summary>
    ///     Converts a <see cref="Result{TValue}" /> to an <see cref="IActionResult" />,
    ///     optionally convert the data before returning
    /// </summary>
    /// <param name="result">The <see cref="Result{TValue}" /> object</param>
    /// <returns>
    ///     The <see cref="IActionResult" /> object
    /// </returns>
    public static IActionResult ToActionResult<TValue, TOut>(this Result<TValue> result,
                                                             Func<TValue, TOut>  converter)
    {
        return result.Match<IActionResult>(
                                           success =>
                                           {
                                               if (converter is not null)
                                               {
                                                   var value = converter(success);

                                                   return new OkObjectResult(value);
                                               }

                                               return new OkObjectResult(success);
                                           },
                                           error =>
                                           {
                                               if (error is IProblemResult problemResult)
                                               {
                                                   return problemResult.ToProblemResult();
                                               }


                                               return error.ToProblemResult();
                                           });
    }

    /// <summary>
    ///     Converts a <see cref="Result{TValue, TError}" /> to an <see cref="IActionResult" />,
    ///     optionally convert the data before returning
    /// </summary>
    /// <param name="result">The <see cref="Result{TValue, TError}" /> object</param>
    /// <returns>
    ///     The <see cref="IActionResult" /> object
    /// </returns>
    public static IActionResult ToActionResult<TValue, TError, TOut>(this Result<TValue, TError> result,
                                                                     Func<TValue, TOut>          converter)
    {
        return result.Match<IActionResult>(
                                           success =>
                                           {
                                               if (converter is not null)
                                               {
                                                   var value = converter(success);

                                                   return new OkObjectResult(value);
                                               }

                                               return new OkObjectResult(success);
                                           },
                                           error =>
                                           {
                                               if (error is IProblemResult problemResult)
                                               {
                                                   return problemResult.ToProblemResult();
                                               }

                                               return new ObjectResult(error)
                                               {
                                                   StatusCode = 500
                                               };
                                           });
    }
}