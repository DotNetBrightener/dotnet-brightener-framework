using AspNet.Extensions.SelfDocumentedProblemResult;
using System.Net;

// ReSharper disable once CheckNamespace
namespace Microsoft.AspNetCore.Mvc;

/// <summary>
///     Shorthand for <see cref="ObjectResult" /> with the given problem detail value
/// </summary>
public class ProblemDetailResult : ObjectResult
{
    /// <summary>
    ///     Initiates a new instance of <see cref="ProblemDetailResult" /> from the given <see cref="IProblemResult" /> value
    /// </summary>
    /// <param name="value">The <see cref="IProblemResult"/> object</param>
    /// <param name="reason">The reason of why the problem / error occured</param>
    /// <param name="instance">The value for <see cref="ProblemDetails.Instance" /></param>
    public ProblemDetailResult(IProblemResult value,
                               string         reason   = null,
                               string         instance = null)
        : base(value.ToProblemDetails(reason, instance))
    {
        StatusCode = value.StatusCode;
    }

    /// <summary>
    ///     Initiates a new instance of <see cref="Exception" /> from the given <see cref="IProblemResult" /> value
    /// </summary>
    /// <param name="value">The <see cref="Exception"/> object</param>
    public ProblemDetailResult(Exception      value,
                               HttpStatusCode statusCode = HttpStatusCode.InternalServerError)
        : base(value.ToProblemDetails(statusCode))
    {
        StatusCode = (int)statusCode;
    }

    /// <summary>
    ///     Initiates a new instance of <see cref="ProblemDetailResult" /> from the given <see cref="ProblemDetails" /> value
    /// </summary>
    /// <param name="value">The <see cref="ProblemDetails"/> object</param>
    public ProblemDetailResult(ProblemDetails value)
        : base(value)
    {
        StatusCode = value.Status;
    }
}