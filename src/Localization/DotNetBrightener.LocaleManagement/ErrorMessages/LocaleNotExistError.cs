using AspNet.Extensions.SelfDocumentedProblemResult.Exceptions;
using System.Net;

namespace LocaleManagement.ErrorMessages;

/// <summary>
///     The error thrown when the locale does not exist
/// </summary>
/// <remarks>
///     If the locale does not exist, it cannot be used
/// </remarks>
public class LocaleNotExistError : BaseProblemDetailsError
{
    public override string DetailReason => "If the locale does not exist, it cannot be used";

    public override string ErrorCode => "LM-0005";
    public override string Summary   => "The error thrown when the locale does not exist";

    public static readonly LocaleNotExistError Instance = new();

    public LocaleNotExistError()
        : base("Locale does not exist", HttpStatusCode.NotFound)
    {
    }
}