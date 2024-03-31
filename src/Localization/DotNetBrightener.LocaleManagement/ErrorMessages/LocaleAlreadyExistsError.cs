using AspNet.Extensions.SelfDocumentedProblemResult.Exceptions;
using System.Net;

namespace LocaleManagement.ErrorMessages;

/// <summary>
///     The error thrown when the locale configuration already exists
/// </summary>
/// <remarks>
///     If the locale configuration for an associated app already exists, it cannot be added again
/// </remarks>
public class LocaleAlreadyExistsError : BaseProblemDetailsError
{
    public override string DetailReason => "If the locale configuration for an associated app already exists, it cannot be added again";

    public override string ErrorCode => "LM-0004";

    public override string Summary   => "The error thrown when the locale configuration already exists";

    public static readonly LocaleAlreadyExistsError Instance = new();

    public LocaleAlreadyExistsError()
        : base("Locale already exists", HttpStatusCode.Conflict)
    {
    }
}