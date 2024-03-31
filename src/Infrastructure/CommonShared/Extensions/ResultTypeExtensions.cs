using System;
using System.Net;
using AspNet.Extensions.SelfDocumentedProblemResult;
using LanguageExts.Results;

// ReSharper disable once CheckNamespace
namespace Microsoft.AspNetCore.Mvc;

public static class ResultTypeExtensions
{
    /// <summary>
    ///     Converts a <see cref="Result{TValue}" /> to an <see cref="IActionResult" />
    /// </summary>
    /// <param name="result">The <see cref="Result{TValue}" /> object</param>
    /// <returns>
    ///     The <see cref="IActionResult" /> object
    /// </returns>
    public static IActionResult ToActionResult<TValue>(this Result<TValue> result,
                                                       HttpStatusCode      statusCode = HttpStatusCode.OK)
    {
        return result.Match<IActionResult>(success => new ObjectResult(success)
                                           {
                                               StatusCode = (int)statusCode
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
    ///     Converts a <see cref="Result{TValue, TError}" /> to an <see cref="IActionResult" />
    /// </summary>
    /// <param name="result">The <see cref="Result{TValue, TError}" /> object</param>
    /// <returns>
    ///     The <see cref="IActionResult" /> object
    /// </returns>
    public static IActionResult ToActionResult<TValue, TError>(this Result<TValue, TError> result,
                                                               HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        return result.Match<IActionResult>(success => new ObjectResult(success)
                                           {
                                               StatusCode = (int)statusCode
                                           },
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
                                                             Func<TValue, TOut>  converter,
                                                             HttpStatusCode      statusCode = HttpStatusCode.OK)
    {
        return result.Match<IActionResult>(success =>
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
                                                                     Func<TValue, TOut> converter,
                                                                     HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        return result.Match<IActionResult>(success =>
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