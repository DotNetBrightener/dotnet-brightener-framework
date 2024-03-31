using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace AspNet.Extensions.SelfDocumentedProblemResult.Exceptions;

/// <summary>
///     Represents the error that occurs during application execution,
///     which can be implicitly returned as a <see cref="Microsoft.AspNetCore.Mvc.ProblemDetails" /> object in the API response
/// </summary>
public abstract class BaseProblemDetailsError : Exception, IProblemResult
{
    protected BaseProblemDetailsError(HttpStatusCode statusCode = HttpStatusCode.InternalServerError)
        : this("", statusCode)
    {
    }

    protected BaseProblemDetailsError(string message, HttpStatusCode statusCode)
        : base(message)
    {
        StatusCode   = (int)statusCode;
        Title        = GetType().Name.CamelFriendly();
        Summary      = GetType().GetXmlDocumentation();
        DetailReason = GetType().GetXmlDocumentationRemarks();
    }

    [NotNull]
    public abstract string ErrorCode { get; }

    public virtual string Summary { get; } = "";

    public virtual string DetailReason { get; }

    public string Title { get; init; }

    public int StatusCode { get; init; }

    IDictionary IProblemResult.Data
    {
        get => Data;
    }

    /// <summary>
    ///     Append extra data to the problem details
    /// </summary>
    /// <param name="key">The key of the data</param>
    /// <param name="value">The data value</param>
    public void AddData(string key, object value)
    {
        Data.Add(key, value);
    }
}