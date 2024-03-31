using System.Collections;

namespace AspNet.Extensions.SelfDocumentedProblemResult;

public interface IProblemResult
{
    /// <summary>
    ///     Define the unique code for the problem. It will be useful for documenting the problem.
    /// </summary>
    string ErrorCode { get; }

    /// <summary>
    ///     A short, human-readable summary of the problem type. It SHOULD NOT change from occurrence to occurrence
    ///     of the problem, except for purposes of localization (e.g., using proactive content negotiation;
    ///     see[RFC7231], Section 3.4).
    /// </summary>
    /// <remarks>
    ///     This will be pulled from the type name to make sure it's not changed
    /// </remarks>
    string Title { get; }

    /// <summary>
    ///     Describes why the problem may occur in detail. It should be pulled from `&lt;remarks&gt;` tag from XML document, if any
    /// </summary>
    string DetailReason { get; }

    /// <summary>
    ///     Describes the problem in general. It should be pulled from `&lt;summary&gt;` tag from XML document, if any
    /// </summary>
    string Summary { get; }

    /// <summary>
    ///     The HTTP status code should be returned for the problem
    /// </summary>
    int StatusCode { get; }

    /// <summary>
    ///     Extra data that can be used to provide more information about the problem
    /// </summary>
    IDictionary Data { get; }
}