using AspNet.Extensions.SelfDocumentedProblemResult.Exceptions;
using System.Net;

namespace LocaleManagement.ErrorMessages;

/// <summary>
///     The error thrown when the dictionary entry already exists
/// </summary>
/// <remarks>
///     If the dictionary entry already exists, it cannot be added again
/// </remarks>
public class DictionaryEntryAlreadyExistsError : BaseProblemDetailsError
{
    public override string ErrorCode    => "LM-0001";

    public override string DetailReason => "If the dictionary entry already exists, it cannot be added again";

    public override string Summary   => "The error thrown when the dictionary entry already exists";

    public DictionaryEntryAlreadyExistsError()
        : base("Dictionary entry has already existed", HttpStatusCode.BadRequest)
    {
    }
}