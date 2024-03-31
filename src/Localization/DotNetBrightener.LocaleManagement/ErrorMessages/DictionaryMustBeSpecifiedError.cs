using AspNet.Extensions.SelfDocumentedProblemResult.Exceptions;
using System.Net;

namespace LocaleManagement.ErrorMessages;

/// <summary>
///     The error thrown  when no dictionary is specified
/// </summary>
/// <remarks>
///     To perform the operation, the identifier of the dictionary must be specified
/// </remarks>
public class DictionaryMustBeSpecifiedError : BaseProblemDetailsError
{
    public override string DetailReason => "To perform the operation, the identifier of the dictionary must be specified";

    public override string ErrorCode => "LM-0003";

    public override string Summary =>
        "The error thrown when no dictionary is specified";

    public static readonly DictionaryMustBeSpecifiedError Instance = new();

    public DictionaryMustBeSpecifiedError()
        : base("Dictionary must be specified", HttpStatusCode.BadRequest)
    {
    }
}