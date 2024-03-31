using AspNet.Extensions.SelfDocumentedProblemResult.Exceptions;
using System.Net;

namespace LocaleManagement.ErrorMessages;

/// <summary>
///     The error thrown when the dictionary entry is not found
/// </summary>
/// <remarks>
///     If the dictionary entry is not found, it cannot be used
/// </remarks>
public class DictionaryEntryNotFoundError : BaseProblemDetailsError
{
    public override string DetailReason => "If the dictionary entry is not found, it cannot be used";

    public override string ErrorCode => "LM-0002";
    public override string Summary   => "The error thrown when the dictionary entry is not found";

    public static readonly DictionaryEntryNotFoundError Instance = new();

    public DictionaryEntryNotFoundError()
        : base("Dictionary Entry Not Found", HttpStatusCode.NotFound)
    {
    }
}