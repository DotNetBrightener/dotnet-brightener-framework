using System.Net;
using AspNet.Extensions.SelfDocumentedProblemResult.Exceptions;

namespace LocaleManagement.ErrorMessages;

/// <summary>
///     The error thrown when the locale is not supported
/// </summary>
public class LocaleNotSupportedError : BaseProblemDetailsError
{
    public override string DetailReason => "If the locale is not supported, it cannot be used";

    public override string ErrorCode => "LM-0006";
    public override string Summary   => "The error thrown when the locale is not supported";

    public LocaleNotSupportedError()
        : base("Locale not supported", HttpStatusCode.BadRequest)
    {
    }
}